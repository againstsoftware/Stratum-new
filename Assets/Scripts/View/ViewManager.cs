using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardLocation = IView.CardLocation;

public class ViewManager : MonoBehaviour, IView
{
    [SerializeField] private ViewPlayer _sagitario, _ygdra, _fungaloth, _overlord;
    [SerializeField] private TableCenter _tableCenter;
    [SerializeField] private GameConfig _config;

    private Dictionary<PlayerCharacter, ViewPlayer> _players;
    private bool _playersInitialized;
    private CameraMovement _cameraMovement;
    private PlayerCharacter _localPlayer;

    private void Awake()
    {
        if (!_playersInitialized) InitPlayers();
    }

    private void Start()
    {
        var comms = ServiceLocator.Get<ICommunicationSystem>();
        comms.OnLocalPlayerChange += SetLocalPlayer;
        SetLocalPlayer(comms.LocalPlayer, comms.Camera);
    }

    private void OnDestroy()
    {
        try
        {
            ServiceLocator.Get<ICommunicationSystem>().OnLocalPlayerChange -= SetLocalPlayer;
        }
        catch {}
    }


    public ViewPlayer GetViewPlayer(PlayerCharacter character)
    {
        if (!_playersInitialized) InitPlayers();
        return _players[character];
    }

    public void PlayCardOnSlotFromPlayer(ACard card, PlayerCharacter actor, CardLocation location, Action callback)
    {
        var playerActor = _players[actor];
        var playerSlotOwner = _players[location.Owner];
        var slot = playerSlotOwner.Territory.Slots[location.SlotIndex];
        playerActor.PlayCardOnSlot(card, slot, callback);
    }

    public void PlaceCardOnSlotFromDeck(ACard card, CardLocation location, Action callback)
    {
        var playerSlotOwner = _players[location.Owner];
        var slot = playerSlotOwner.Territory.Slots[location.SlotIndex];
        playerSlotOwner.PlaceCardFromDeck(card, slot, callback);
    }

    public void PlaceInitialCards(IReadOnlyList<(ACard card, CardLocation location)> cardsAndLocations, Action callback)
    {
        StartCoroutine(PlaceInitialCardsAux(cardsAndLocations, callback));
    }


    public void GrowPopulationCardEcosystem(CardLocation location, Action callback)
    {
        var playerOwner = _players[location.Owner];
        var slot = playerOwner.Territory.Slots[location.SlotIndex]; //la carta de mas arriba del slot
        var card = slot.Cards[^1].Card;

        // var newCardGO = Instantiate(_config.CardPrefab, slot.SnapTransform.position, slot.SnapTransform.rotation);
        // var newPlayableCard = newCardGO.GetComponent<PlayableCard>();
        // newPlayableCard.InitializeOnSlot(card, location.Owner, slot);
        // slot.AddCardOnTop(newPlayableCard);

        StartCoroutine(AppearCard(card, slot, null, true));

        StartCoroutine(DelayCall(callback, 1f)); //de prueba
    }


    public void KillPopulationCardEcosystem(CardLocation location, Action callback)
    {
        var playerOwner = _players[location.Owner];
        var slot = playerOwner.Territory.Slots[location.SlotIndex];
        var card = slot.Cards[^1]; //la carta de mas arriba del slot
        if (card.Card is not PopulationCard) throw new Exception("Error! La carta a matar no es de poblacion");
        slot.RemoveCard(card);
        // Destroy(card.gameObject);
        card.DestroyCard(null);
        StartCoroutine(DelayCall(callback, 1f)); //de prueba
    }

    public void Discard(PlayerCharacter actor, Action callback)
    {
        var playerActor = _players[actor];
        playerActor.DiscardCardFromHand(callback);
    }

    public void DrawCards(IReadOnlyDictionary<PlayerCharacter, IReadOnlyList<ACard>> cardsDrawn, Action callback)
    {
        StartCoroutine(Draw(cardsDrawn, callback));
    }

    public void SwitchCamToOverview(PlayerCharacter actor, Action callback)
    {
        if (actor == _localPlayer)
            StartCoroutine(DelayCall(callback, .01f));
        
        else
            _cameraMovement.ChangeToOverview(() => StartCoroutine(DelayCall(callback, .01f)));
    }

    public void GrowPopulation(CardLocation location, Population population, Action callback,
        bool isEndOfAction = false)
    {
        var card = _config.GetPopulationCard(population);
        var playerOwner = _players[location.Owner];
        var slot = playerOwner.Territory.Slots[location.SlotIndex];

        // var newCardGO = Instantiate(_config.CardPrefab, slot.SnapTransform.position, slot.SnapTransform.rotation);
        // var newPlayableCard = newCardGO.GetComponent<PlayableCard>();
        // newPlayableCard.InitializeOnSlot(card, location.Owner, slot);
        // slot.AddCardOnTop(newPlayableCard);

        StartCoroutine(AppearCard(card, slot, null, true));

        StartCoroutine(DelayCall(() => { callback?.Invoke(); }, .5f)); //de prueba
    }

    public void GrowMushroom(CardLocation location, Action callback, bool isEndOfAction = false)
    {
        // var card = _config.Mushroom;
        var playerOwner = _players[location.Owner];
        var slot = playerOwner.Territory.Slots[location.SlotIndex];
        //
        // var newCardGO = Instantiate(_config.CardPrefab, slot.SnapTransform.position, slot.SnapTransform.rotation);
        // var newPlayableCard = newCardGO.GetComponent<PlayableCard>();
        // newPlayableCard.InitializeOnSlot(card, location.Owner, slot);
        // slot.AddCardAtTheBottom(newPlayableCard);


        StartCoroutine(AppearCard(_config.Mushroom, slot, callback, false));


        // StartCoroutine(DelayCall(() => { callback?.Invoke(); }, .5f)); //de prueba
    }

    public void GrowMacrofungi(CardLocation[] locations, Action callback)
    {
        if (locations.Length != 3) throw new Exception("Error! != 3 setas para macrohongo en view!");

        var token = GetViewPlayer(PlayerCharacter.Fungaloth).Token;
        token.Play(_tableCenter, () => { StartCoroutine(DestroyMushroomsAndGrowMacrofungi(locations, callback)); });
    }

    public void PlaceConstruction(CardLocation plant1Location, CardLocation plant2Location, Action callback)
    {
        var ownerPlayer = GetViewPlayer(plant1Location.Owner);
        var territory = ownerPlayer.Territory;
        if (territory.HasConstruction)
            throw new Exception("Error!!! Construyendo en territorio ya construido. (view)");

        var token = GetViewPlayer(PlayerCharacter.Overlord).Token;

        token.Play(_tableCenter,
            () => { StartCoroutine(DestroyPlantsAndConstruct(plant1Location, plant2Location, callback)); });
    }


    public void PlayAndDiscardInfluenceCard(PlayerCharacter actor, AInfluenceCard card, CardLocation location,
        Action callback,
        bool isEndOfAction = false)
    {
        var playerActor = _players[actor];

        IActionReceiver receiver;

        if (location.IsTableCenter) receiver = _tableCenter;
        else
        {
            var receiverOwner = _players[location.Owner];
            receiver = location.IsTerritory
                ? receiverOwner.Territory
                : receiverOwner.Territory.Slots[location.SlotIndex];
        }

        playerActor.PlayAndDiscardInfluenceCard(card, receiver, callback, isEndOfAction);
    }

    public void MovePopulationToEmptySlot(PlayerCharacter actor, CardLocation from, CardLocation to, Action callback)
    {
        var playerActor = _players[actor];
        var playerOwner = _players[from.Owner];
        var slot = playerOwner.Territory.Slots[from.SlotIndex];
        var card = playerOwner.Territory.Slots[from.SlotIndex].Cards[from.CardIndex];
        if (card.Card is not PopulationCard) throw new Exception("Error! La carta a mover no es de poblacion");
        slot.RemoveCard(card);

        var targetOwner = _players[to.Owner];
        var targetSlot = targetOwner.Territory.Slots[to.SlotIndex];

        card.Initialize(card.Card, to.Owner);

        if (card.InfluenceCardOnTop is not null)
            card.InfluenceCardOnTop.transform.parent = card.transform;

        card.Play(targetSlot, () =>
        {
            if (card.InfluenceCardOnTop is not null)
                card.InfluenceCardOnTop.transform.parent = null;
            callback?.Invoke();
        });
    }

    public void ShowBirds(CardLocation from, CardLocation to, Action callback)
    {
        SoundManager.Instance.PlaySound("Birds");

        var playerOwner = _players[from.Owner];
        var slot = playerOwner.Territory.Slots[from.SlotIndex];
        var card = playerOwner.Territory.Slots[from.SlotIndex].Cards[from.CardIndex];

        var targetOwner = _players[to.Owner];
        var targetSlot = targetOwner.Territory.Slots[to.SlotIndex];

        var position = card.transform.position;
        position.y = _config.BirdsPrefab.transform.position.y;
        var birds = Instantiate(_config.BirdsPrefab, position, Quaternion.identity);

        var targetPos = targetSlot.transform.position;
        targetPos.y = position.y;
        birds.transform.LookAt(targetPos);

        Destroy(birds, 4f);

        StartCoroutine(DelayCall(callback, .01f));
    }

    public void ShowFireworks(CardLocation location, Action callback)
    {
        SoundManager.Instance.PlaySound("Fireworks");

        var playerOwner = _players[location.Owner];
        var card = playerOwner.Territory.Slots[location.SlotIndex].Cards[location.CardIndex];

        var fireworks = Instantiate(_config.FireworksPrefab, card.transform.position,
            _config.FireworksPrefab.transform.rotation);

        Destroy(fireworks, 6f);
        StartCoroutine(DelayCall(callback, .6f));
    }

    public void ShowDirt(CardLocation location, Action callback)
    {
        var playerOwner = _players[location.Owner];
        var slot = playerOwner.Territory.Slots[location.SlotIndex];

        var dirt = Instantiate(_config.DirtPrefab, slot.transform.position,
            _config.DirtPrefab.transform.rotation);

        Destroy(dirt, 3f);
        StartCoroutine(DelayCall(callback, .6f));
    }

    public void ShowFragrance(CardLocation from, CardLocation to, Action callback)
    {
        var cardOwner = _players[from.Owner];
        var card = cardOwner.Territory.Slots[from.SlotIndex].Cards[from.CardIndex];

        var slotOwner = _players[to.Owner];
        var slot = slotOwner.Territory.Slots[to.SlotIndex];

        var position = slot.transform.position;
        position.y = _config.FragrancePrefab.transform.position.y;
        
        var fragrance = Instantiate(_config.FragrancePrefab, position, Quaternion.identity);

        var targetPos = card.transform.position;
        targetPos.y = position.y;
        fragrance.transform.LookAt(targetPos);

        Destroy(fragrance, 5f);

        StartCoroutine(DelayCall(callback, 1.75f));
    }
    
    public void ShowAppetizingMushroom(CardLocation from, CardLocation to, Action callback)
    {
        var cardOwner = _players[from.Owner];
        var card = cardOwner.Territory.Slots[from.SlotIndex].Cards[from.CardIndex];

        var slotOwner = _players[to.Owner];
        var slot = slotOwner.Territory.Slots[to.SlotIndex];

        var position = slot.transform.position;
        position.y = _config.FragrancePrefab.transform.position.y;
        
        var appMush = Instantiate(_config.AppetizingMushroomPrefab, position, Quaternion.identity);

        var targetPos = card.transform.position;
        targetPos.y = position.y;
        appMush.transform.LookAt(targetPos);

        Destroy(appMush, 5f);

        StartCoroutine(DelayCall(callback, 1.75f));
    }


    public void SpinTurnMarker(Action callback)
    {
        var turnMarker = FindObjectOfType<TurnMarker>();
        var onTurn = ServiceLocator.Get<ITurnSystem>().PlayerOnTurn;
        turnMarker.OnTurnChanged(PlayerCharacter.None);
        StartCoroutine(DelayCall(callback, .01f));

        StartCoroutine(DelayCall(() =>
        {
            if(turnMarker.CurrentState is TurnMarker.State.Spinning)
                turnMarker.OnTurnChanged(onTurn);
        }, 1.2f));
    }


    public void PlaceInfluenceOnPopulation(PlayerCharacter actor, AInfluenceCard influenceCard, CardLocation location,
        Action callback, bool isEndOfAction = false)
    {
        var playerActor = _players[actor];
        var playerOwner = _players[location.Owner];
        var slot = playerOwner.Territory.Slots[location.SlotIndex];
        var card = playerOwner.Territory.Slots[location.SlotIndex].Cards[location.CardIndex];

        playerActor.PlaceInfluenceOnPopulation(influenceCard, card, callback, isEndOfAction);
    }


    public void GiveRabies(PlayerCharacter actor, CardLocation location, Action callback)
    {
        var playerActor = _players[actor];
        callback?.Invoke();
    }

    public void MakeOmnivore(PlayerCharacter actor, CardLocation location, Action callback)
    {
        var playerActor = _players[actor];
        callback?.Invoke();
    }

    public void PutLeash(PlayerCharacter actor, CardLocation location, Action callback)
    {
        var playerActor = _players[actor];
        callback?.Invoke();
    }

    public void DestroyInTerritory(PlayerCharacter territoryOwner, Action callback,
        Predicate<ACard> filter = null)
    {
        var playerOwner = _players[territoryOwner];
        Stack<PlayableCard> toBeRemoved = new();
        foreach (var slot in playerOwner.Territory.Slots)
        {
            foreach (var card in slot.Cards)
            {
                if (filter is not null && filter(card.Card)) continue;
                toBeRemoved.Push(card);
            }

            while (toBeRemoved.Any())
            {
                var card = toBeRemoved.Pop();

                DestroyCard(card, slot, null);
            }
        }


        if (playerOwner.Territory.HasConstruction)
        {
            DestroyConstruction(territoryOwner, callback, false);
            return;
        }

        StartCoroutine(DelayCall(() => { callback?.Invoke(); }, 0.75f));
    }

    public void DestroyConstruction(PlayerCharacter territoryOwner, Action callback, bool isIvy)
    {
        var playerOwner = _players[territoryOwner];
        playerOwner.Territory.DestroyConstruction(isIvy);
        StartCoroutine(DelayCall(() => { callback?.Invoke(); }, 0.75f));
    }

    public void KillPlacedCard(CardLocation location, Action callback)
    {
        var playerOwner = _players[location.Owner];
        var slot = playerOwner.Territory.Slots[location.SlotIndex];
        var card = slot.Cards[location.CardIndex];
        // if (card.Card is not PopulationCard) throw new Exception("Error! La carta a matar no es de poblacion");
        DestroyCard(card, slot, callback);
        // StartCoroutine(DelayCall(() => { callback?.Invoke(); }, .5f)); //de prueba
    }


    public void DiscardInfluenceFromPopulation(CardLocation location, Action callback)
    {
        var playerOwner = _players[location.Owner];
        var slot = playerOwner.Territory.Slots[location.SlotIndex];
        var populationCard = slot.Cards[location.CardIndex];
        var influenceCard = populationCard.InfluenceCardOnTop;
        playerOwner.DiscardInfluenceFromPopulation(influenceCard, callback);
    }


    private void SetLocalPlayer(PlayerCharacter localPlayer, Camera cam)
    {
        if (localPlayer is PlayerCharacter.None) return;
        _localPlayer = localPlayer;
        _cameraMovement = cam.GetComponent<CameraMovement>();
    }


    private void InitPlayers()
    {
        _playersInitialized = true;

        _players = new()
        {
            { PlayerCharacter.Sagitario, _sagitario },
            { PlayerCharacter.Ygdra, _ygdra },
            { PlayerCharacter.Fungaloth, _fungaloth },
            { PlayerCharacter.Overlord, _overlord },
            { PlayerCharacter.None, null },
        };

        foreach (var (character, viewPlayer) in _players)
        {
            if (character is PlayerCharacter.None) continue;
            viewPlayer.Initialize(character);
        }
    }

    private IEnumerator PlaceInitialCardsAux(IReadOnlyList<(ACard card, CardLocation location)> cardsAndLocations,
        Action callback)
    {
        foreach (var (card, location) in cardsAndLocations)
        {
            bool isDone = false;
            PlaceCardOnSlotFromDeck(card, location, () => isDone = true);
            yield return new WaitUntil(() => isDone);
        }

        callback?.Invoke();
    }

    private IEnumerator DelayCall(Action a, float delay)
    {
        yield return new WaitForSeconds(delay);
        a?.Invoke();
    }

    private IEnumerator Draw(IReadOnlyDictionary<PlayerCharacter, IReadOnlyList<ACard>> cardsDrawn, Action callback)
    {
        bool[] arePlayersFinished = new bool[cardsDrawn.Count];
        int i = 0;
        foreach (var (character, cards) in cardsDrawn)
        {
            // Debug.Log($"va a robar: {character}");
            int index = i++;
            arePlayersFinished[index] = false;
            GetViewPlayer(character).DrawCards(cards, () =>
            {
                // Debug.Log($"ha robado {character}");
                arePlayersFinished[index] = true;
            });
        }

        yield return new WaitUntil(() => arePlayersFinished.All(pf => pf));
        // Debug.Log("cartas robadas");
        callback?.Invoke();
    }

    private IEnumerator DestroyMushroomsAndGrowMacrofungi(CardLocation[] locations, Action callback)
    {
        CardLocation location = default;
        ViewPlayer playerOwner;
        SlotReceiver slot;
        foreach (var l in locations)
        {
            location = l;
            playerOwner = _players[location.Owner];
            slot = playerOwner.Territory.Slots[location.SlotIndex];
            var card = slot.Cards[0]; //la carta de mas abajo del slot
            if (card.Card is not MushroomCard) throw new Exception("Error! La carta para macrohongo no es seta!");

            DestroyCard(card, slot, null);

            yield return new WaitForSeconds(.5f);
        }

        //la ultima location es donde crece el macrohongo
        playerOwner = _players[location.Owner];
        slot = playerOwner.Territory.Slots[location.SlotIndex];

        // var newCardGO = Instantiate(_config.CardPrefab, slot.SnapTransform.position, slot.SnapTransform.rotation);
        // var newPlayableCard = newCardGO.GetComponent<PlayableCard>();
        // newPlayableCard.InitializeOnSlot(_config.Macrofungi, location.Owner, slot);
        // slot.AddCardAtTheBottom(newPlayableCard);

        StartCoroutine(AppearCard(_config.Macrofungi, slot, callback, false));

        // yield return new WaitForSeconds(.5f);
        // callback?.Invoke();
    }

    private IEnumerator DestroyPlantsAndConstruct(CardLocation plant1Location, CardLocation plant2Location,
        Action callback)
    {
        var territory = GetViewPlayer(plant1Location.Owner).Territory;

        var slot1 = territory.Slots[plant1Location.SlotIndex];
        var card1 = slot1.Cards[plant1Location.CardIndex];

        var slot2 = territory.Slots[plant2Location.SlotIndex];
        var card2 = slot2.Cards[plant2Location.CardIndex];

        DestroyCard(card1, slot1, null);
        yield return new WaitForSeconds(.25f);
        DestroyCard(card2, slot2, null);
        yield return new WaitForSeconds(.25f);


        territory.BuildConstruction();

        yield return new WaitForSeconds(.5f);
        callback?.Invoke();
    }

    private void DestroyCard(PlayableCard card, SlotReceiver slot, Action callback)
    {
        slot.RemoveCard(card);
        card.DestroyCard(callback);
    }

    private IEnumerator AppearCard(ACard card, SlotReceiver slot, Action callback, bool onTop)
    {
        var newCardGO = Instantiate(_config.CardPrefab, slot.SnapTransform.position, slot.SnapTransform.rotation);
        var materializableCard = newCardGO.GetComponent<AppearableCard>();
        var newPlayableCard = newCardGO.GetComponent<PlayableCard>();
        newPlayableCard.InitializeOnSlot(card, slot.Owner, slot);

        bool isMaterialized = false;
        materializableCard.AppearCard(card, () => isMaterialized = true);

        yield return new WaitUntil(() => isMaterialized);

        if (onTop) slot.AddCardOnTop(newPlayableCard);
        else slot.AddCardAtTheBottom(newPlayableCard);

        callback?.Invoke();
    }
}