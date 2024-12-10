using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class ViewPlayer : MonoBehaviour
{
    [field: SerializeField] public TerritoryReceiver Territory { get; private set; }
    [field: SerializeField] public DiscardPileReceiver DiscardPile { get; private set; }
    [field: SerializeField] public PlayableToken Token { get; private set; }
    [field: SerializeField] public Camera MainCamera { get; private set; }
    [field: SerializeField] public Camera UICamera { get; private set; }
    [field: SerializeField] public GameObject Mesh { get; private set; }

    public readonly List<PlayableCard> Cards = new(5);

    public bool IsLocalPlayer
    {
        get => _isLocalPlayer;
        set
        {
            _isLocalPlayer = value;
            Mesh.SetActive(!_isLocalPlayer);
        }
    }

    private bool _isLocalPlayer;

    public PlayerCharacter Character { get; private set; } = PlayerCharacter.None;

    [SerializeField] private Transform _hand;
    [SerializeField] private Transform _deckSnap;
    [SerializeField] private Transform[] _cardLocations;
    [SerializeField] private GameConfig _config;

    private PlayableCard _droppedCard;

    private IKCardInteractionController _ikController;
    private Animator _camAnimator;

    private static readonly int _dieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        _ikController = Mesh.GetComponent<IKCardInteractionController>();

        if (Mesh.TryGetComponent<Animator>(out var animator))
            animator.Play("Idle", 0, Random.Range(0f, 1f));

        _camAnimator = MainCamera.GetComponent<Animator>();
        _camAnimator.enabled = false;
    }

    public void Initialize(PlayerCharacter character)
    {
        if (Character is not PlayerCharacter.None) throw new Exception("ya inicializado!!");
        Character = character;
    }

    public void DrawCards(IReadOnlyList<ACard> cards, Action callback)
    {
        StartCoroutine(DrawCardsAux(cards, callback));
    }

    public void PlayCardOnSlot(ACard card, SlotReceiver slot, Action callback)
    {
        PlayableCard playableCard = null;
        if (IsLocalPlayer)
        {
            playableCard = _droppedCard;
            if (card != playableCard.Card) throw new Exception("carta diferente en el view!!");
        }
        else
        {
            // playableCard = Cards[0];
            playableCard = GetClosestCardToReceiver(slot);
            playableCard.SetCard(card);
            _ikController.AssignTarget(playableCard.transform);
        }

        playableCard.Play(slot, () =>
        {
            if (!IsLocalPlayer) _ikController.ResetTarget(callback);
            else callback?.Invoke();
        }, true, !IsLocalPlayer);
    }

    public void PlayAndDiscardInfluenceCard(AInfluenceCard card, IActionReceiver receiver, Action callback,
        bool isEndOfAction = false)
    {
        PlayableCard playableCard = null;

        if (IsLocalPlayer)
        {
            playableCard = _droppedCard;
            if (card != playableCard.Card) throw new Exception("carta diferente en el view!!");
        }
        else
        {
            // playableCard = Cards[0];
            playableCard = GetClosestCardToReceiver(receiver);
            playableCard.SetCard(card);
            _ikController.AssignTarget(playableCard.transform);
        }

        playableCard.Play(receiver, () =>
        {
            if (!IsLocalPlayer) _ikController.ResetTarget(null);
            StartCoroutine(DelayCall(() =>
            {
                playableCard.Play(DiscardPile, () => //probablemente la destruyamos in place
                {
                    StartCoroutine(DestroyDiscardedCard(playableCard.gameObject, callback));
                }, isEndOfAction);
            }, .3f)); //delayeamos 1 frame (mas) (perdon)
        }, false, !IsLocalPlayer);
    }

    public void DiscardCardFromHand(Action callback)
    {
        PlayableCard playableCard;
        if (IsLocalPlayer)
        {
            playableCard = _droppedCard;
        }
        else
        {
            // playableCard = Cards[0];
            playableCard = GetClosestCardToReceiver(DiscardPile);
            _ikController.AssignTarget(playableCard.transform);
        }

        playableCard.Play(DiscardPile, () =>
        {
            if (!IsLocalPlayer) _ikController.ResetTarget(null);
            StartCoroutine(DestroyDiscardedCard(playableCard.gameObject, callback));
        }, true, !IsLocalPlayer);
    }

    public void DiscardInfluenceFromPopulation(PlayableCard influenceCard, Action callback)
    {
        influenceCard.Play(DiscardPile,
            () => { StartCoroutine(DestroyDiscardedCard(influenceCard.gameObject, callback)); });
    }

    public void PlaceCardFromDeck(ACard card, SlotReceiver slot, Action callback)
    {
        var newCardGO = Instantiate(_config.CardPrefab, _deckSnap.position, _deckSnap.rotation);
        var newPlayableCard = newCardGO.GetComponent<PlayableCard>();
        newPlayableCard.Initialize(card, Character);

        newPlayableCard.Play(slot, callback);
    }

    public void PlaceInfluenceOnPopulation(AInfluenceCard influence, PlayableCard population, Action callback,
        bool isEndOfAction = false)
    {
        PlayableCard playableCard = null;

        if (IsLocalPlayer)
        {
            playableCard = _droppedCard;
            if (influence != playableCard.Card) throw new Exception("carta diferente en el view!!");
        }
        else
        {
            // playableCard = Cards[0];
            playableCard = GetClosestCardToReceiver(population);
            playableCard.SetCard(influence);
            _ikController.AssignTarget(playableCard.transform);
        }

        playableCard.Play(population, () =>
        {
            if (!IsLocalPlayer) _ikController.ResetTarget(callback);
            else callback?.Invoke();
        }, isEndOfAction, !IsLocalPlayer);
    }

    public void Die()
    {
        StartCoroutine(DieCoroutine());
    }

    public void Win()
    {
        StartCoroutine(WinCoroutine());
    }

    private IEnumerator DieCoroutine()
    {
        yield return DestroyCardsInHand();

        if (!_isLocalPlayer)
        {
            Mesh.GetComponent<Animator>().SetTrigger(_dieHash);
            yield return new WaitForSeconds(2f);
        }

        else
        {
            _camAnimator.enabled = true;
            yield return null;
            _camAnimator.Play(_dieHash);

            MainCamera.GetComponent<Volume>().profile.TryGet<ColorAdjustments>(out var colorAdjustments);
            float initSaturation = colorAdjustments.saturation.value;
            float progress = 0f;
            while (progress <= 1f)
            {
                colorAdjustments.saturation.value = Mathf.Lerp(initSaturation, -100f, progress);
                progress += Time.deltaTime / 2f;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1f);
        SceneTransition.Instance.TransitionToScene("MainMenu");
    }

    private IEnumerator WinCoroutine()
    {
        if (IsLocalPlayer)
        {
            ServiceLocator.Get<IInteractionSystem>().Disable();
            ServiceLocator.Get<IInteractionSystem>().DisableInput();
        }

        yield return DestroyCardsInHand();
        yield return new WaitForSeconds(3f);
        SceneTransition.Instance.TransitionToScene("MainMenu");
    }

    private IEnumerator DestroyCardsInHand()
    {
        bool[] destroyed = new bool[Cards.Count];
        for (int i = 0; i < Cards.Count; i++)
        {
            var card = Cards[i];
            var index = i;
            card.DestroyCard(() => destroyed[index] = true);
        }

        _ikController.DropHandTragets();
        // yield return new WaitUntil(() => destroyed.All(d => d));
        yield return new WaitForSeconds(1f);
        _ikController.enabled = false;
    }


    private IEnumerator DrawCardsAux(IReadOnlyList<ACard> cards, Action callback)
    {
        // Debug.Log($"empezando a robar: {Character}, {cards.Count} cartas.");
        foreach (var card in cards)
        {
            var newCardGO = Instantiate(_config.CardPrefab, _deckSnap.position, _deckSnap.rotation, _hand);
            var newPlayableCard = newCardGO.GetComponent<PlayableCard>();

            // if (!IsLocalPlayer) card = null;
            newPlayableCard.Initialize(card, Character);

            Cards.Add(newPlayableCard);
            newPlayableCard.OnCardPlayed += OnCardPlayed;
            newPlayableCard.OnItemDrag += OnCardDragged;
            newPlayableCard.OnItemDrop += OnCardDropped;
            // newPlayableCard.IndexInHand = Cards.Count - 1;

            var location = _cardLocations[Cards.Count - 1];

            bool isDone = false;

            // Debug.Log($"robando: {Character}");

            newPlayableCard.DrawTravel(location, () => { isDone = true; });

            yield return new WaitUntil(() => isDone);
            yield return null;
        }

        yield return ReposCardsInHand();

        callback?.Invoke();
    }


    private void OnCardPlayed(PlayableCard card)
    {
        card.OnCardPlayed -= OnCardPlayed;
        Cards.Remove(card);
        StartCoroutine(ReposCardsInHand());
    }

    private void OnCardDragged(APlayableItem item)
    {
        if (!IsLocalPlayer) return;
        var card = item as PlayableCard;
        int newIndex = Cards.Count - 1;
        Cards.Remove(card);
        Cards.Add(card); //metemos la carta al final
        card.InHandPosition = _cardLocations[newIndex].position;
        card.InHandRotation = _cardLocations[newIndex].rotation;
        StartCoroutine(ReposCardsInHand(new[] { card })); //repos a todas las cartas menos a ella
    }

    private void OnCardDropped(APlayableItem item)
    {
        if (!IsLocalPlayer) return;
        var card = item as PlayableCard;
        _droppedCard = card;
    }

    private IEnumerator ReposCardsInHand(PlayableCard[] exclude = null)
    {
        int offset = (5 - Cards.Count) / 2;

        for (int i = 0; i < Cards.Count; i++)
        {
            bool cardReposed = false;
            var card = Cards[i];
            var index = Mathf.Min(Cards.Count, i + offset);
            var newLocation = _cardLocations[index];

            if (exclude is not null && exclude.Contains(card)) continue;
            if (card.transform.position == newLocation.position) continue;

            card.ReposInHand(newLocation, () => cardReposed = true);

            yield return new WaitUntil(() => cardReposed);
        }
    }

    private IEnumerator DestroyDiscardedCard(GameObject card, Action callback = null)
    {
        yield return null;
        Destroy(card);

        DiscardPile.ShowDiscarded();
        callback?.Invoke();
    }

    private IEnumerator DelayCall(Action a, float delay)
    {
        yield return new WaitForSeconds(delay);
        a?.Invoke();
    }


    private PlayableCard GetClosestCardToReceiver(IActionReceiver receiver)
    {
        var receiverPos = receiver.GetSnapTransform(Character).position;
        PlayableCard closest = Cards[0];
        float closestDistance = float.MaxValue;
        foreach (var card in Cards)
        {
            var distance = Vector3.Distance(receiverPos, card.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = card;
            }
        }

        return closest;
    }
}