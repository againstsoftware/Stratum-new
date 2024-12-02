using System;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public class PlayableCard : APlayableItem, IActionReceiver, IRulebookEntry
{
    public override bool OnlyVisibleOnOverview => false;
    public override bool CanInteractWithoutOwnership => _canInteractWithoutOwnership;

    public override AActionItem ActionItem => Card;
    public bool IsDropEnabled { get; private set; } = false;

    public ACard Card { get; private set; }
    [field: SerializeField] public Transform SnapTransform { get; private set; }
    public Transform GetSnapTransform(PlayerCharacter _) => SnapTransform;

    public string GetName() => Card.Name;
    public string GetDescription() => Card.Description;
    public int IndexOnSlot { get; set; } = -1;

    public SlotReceiver SlotWherePlaced { get; private set; }
    public PlayableCard CardWherePlaced { get; private set; }
    public PlayableCard InfluenceCardOnTop { get; private set; }

    public Action<PlayableCard> OnCardPlayed;


    [SerializeField] private float _drawTravelDuration, _reposInHandTravelDuration;
    [SerializeField] private float _closestCardZ;
    [SerializeField] private MeshRenderer _mesh;
    [SerializeField] private TextMeshProUGUI _nameText;

    
    [SerializeField] private Material _transparentObverse;
    [SerializeField] private Material _transparentReverse;

    private float _startZ;
    private bool _canInteractWithoutOwnership = false;

    private Transform _hand;
    // private static readonly int _mainTexture = Shader.PropertyToID("_Main_Texture");

    private Material _opaqueObverse, _opaqueReverse;
    
    
    protected override void Awake()
    {
        base.Awake();
        
        _hand = transform.parent;
        _opaqueReverse = _mesh.materials[0];
        _opaqueObverse = _mesh.materials[1];

        _transparentObverse = Instantiate(_transparentObverse); //reverso no hace falta pq no lo cambiamos
    }


    public void Play(IActionReceiver playLocation, Action onPlayedCallback, bool isEndOfAction = true)
    {
        OnCardPlayed?.Invoke(this);

        bool isAlreadyPlayed = CurrentState is not State.Playable && IsOnPlayLocation(playLocation);

        if (isAlreadyPlayed)
        {
            if (Card is AInfluenceCard { IsPersistent: true })
            {
                if(playLocation is PlayableCard pc) OnPersistentPlayed(pc);
                else if(playLocation is DiscardPileReceiver) OnPersistendDiscarded();
            }
            else if (Card is MushroomCard or MacrofungiCard)
            {
                OnMushroomOrMacrofungiPlayed(playLocation);
            }

            if (isEndOfAction)
            {
                if (Card is PopulationCard) OnPopulationPlayed(playLocation);
            }

            onPlayedCallback();
            return;
        }

        //no se ha jugado visualmente a la mesa
        Travel(playLocation.GetSnapTransform(Owner), _playTravelDuration, State.Played, () =>
        {
            if (Card is AInfluenceCard { IsPersistent: true })
            {
                if(playLocation is PlayableCard pc) OnPersistentPlayed(pc);
                else if(playLocation is DiscardPileReceiver) OnPersistendDiscarded();
            }
            else if (Card is MushroomCard or MacrofungiCard)
            {
                OnMushroomOrMacrofungiPlayed(playLocation);
            }

            if (isEndOfAction)
            {
                if (Card is PopulationCard) OnPopulationPlayed(playLocation);
            }
            

            onPlayedCallback();
        });
    }

    //PARA CARTAS DE INFLUENCIA QUE SON DESTRUIDAS ANTES DE REPORTART EL CALLBACK DE FIN DE ACCION


    private void OnPopulationPlayed(IActionReceiver playLocation)
    {
        if (_dbWasObv)
        {
            Debug.Log("poblacion de overlord jugada");
        }
        CurrentState = State.Played;
        IsDropEnabled = true;
        _canInteractWithoutOwnership = true;

        SlotWherePlaced = playLocation as SlotReceiver;
        //CardWherePlaced = playLocation as PlayableCard; //no se deberia jugar a una poblacion sobre otra carta

        if (SlotWherePlaced is not null) SlotWherePlaced.AddCardOnTop(this);

        if (playLocation is DiscardPileReceiver)
        {
            //se puede destruir aqui tal vez? en vez de en el viewplayer
        }
    }

    private void OnMushroomOrMacrofungiPlayed(IActionReceiver playLocation)
    {
        CurrentState = State.Played;
        IsDropEnabled = true;
        _canInteractWithoutOwnership = true;

        SlotWherePlaced = playLocation as SlotReceiver;
        if (SlotWherePlaced is not null) SlotWherePlaced.AddCardOnTop(this);
    }
    

    private void OnPersistentPlayed(PlayableCard cardWherePlaced)
    {
        if (_dbWasObv)
        {
            Debug.Log("influencia persistente de overlord jugada");
        }
        CurrentState = State.Played;
        IsDropEnabled = false;
        _canInteractWithoutOwnership = true;
        transform.parent = cardWherePlaced.transform;
        CardWherePlaced = cardWherePlaced;
        CardWherePlaced.AddInfluenceCardOnTop(this);
    }

    private void OnPersistendDiscarded()
    {
        transform.parent = null;
        if(CardWherePlaced is not null) CardWherePlaced.RemoveInfluenceCardOnTop();
        CardWherePlaced = null;
    }


    public override void OnSelect()
    {
        _startZ = transform.localPosition.z;
        if (CurrentState is State.Playable)
            transform.localPosition = new(transform.localPosition.x, transform.localPosition.y, _closestCardZ);
        base.OnSelect();
    }

    public override void OnDeselect()
    {
        if (_destroyed) return;
        base.OnDeselect();
        if (CurrentState is State.Playable)
            transform.localPosition = new(transform.localPosition.x, transform.localPosition.y, _startZ);
    }

    public void OnDraggingSelect()
    {
        OnSelect();
    }

    public void OnDraggingDeselect()
    {
        OnDeselect();
    }

    public void OnChoosingSelect()
    {
        OnSelect();
    }

    public void OnChoosingDeselect()
    {
        OnDeselect();
    }


    public override void OnDrag()
    {
        base.OnDrag();

        SetTransparency(true);
    }

    public override void OnDrop(IActionReceiver dropLocation)
    {
        base.OnDrop(dropLocation);
        transform.parent = null;
        transform.rotation = dropLocation.GetSnapTransform(Owner).rotation; //?????? hay que cambiarlo
        SetTransparency(false);
    }

    public override void OnDragCancel()
    {
        transform.parent = _hand;
        base.OnDragCancel();
        SetTransparency(false);
    }


    public Receiver GetReceiverStruct(ValidDropLocation actionDropLocation)
    {
        //esto esta regular pq una carta de influencia no deberia ser receiver
        //lo que hace es devolver su card where placed como receiver
        return CardWherePlaced is not null
            ? CardWherePlaced.GetReceiverStruct(actionDropLocation)
            : new(actionDropLocation, Owner, SlotWherePlaced.IndexOnTerritory, IndexOnSlot);
    }


    public void DrawTravel(Transform target, Action callback, bool dbIsOv)
    {
        InHandPosition = target.position;
        InHandRotation = target.rotation;
        Travel(target, _drawTravelDuration, State.Playable, callback, dbIsOv);
    }

    public void ReposInHand(Transform target, Action callback)
    {
        Travel(target, _reposInHandTravelDuration, State.Playable, callback);
    }

    public void Initialize(ACard card, PlayerCharacter owner, State initialState = State.Playable)
    {
        // if (Card is not null) throw new Exception("carta ya asignada no se puede inicializar!");
        if (card is null)
        {
            return;
        }

        SetCard(card);
        Owner = owner;
        if (_dbWasObv)
        {
            Debug.Log("carta de overlord inicializada");
        }
        CurrentState = initialState;
    }

    public void InitializeOnSlot(ACard card, PlayerCharacter slotOwner, SlotReceiver slot)
    {
        // if (Card is not null) throw new Exception("carta ya asignada no se puede inicializar!");
        if (card is null)
        {
            return;
        }

        SetCard(card);
        Owner = slotOwner;
        if (_dbWasObv)
        {
            Debug.Log("carta de overlord inicializada en slot");
        }
        CurrentState = State.Played;
        IsDropEnabled = true;
        _canInteractWithoutOwnership = true;
        SlotWherePlaced = slot;
    }


    public void SetCard(ACard card)
    {
        Card = card;
        // _mesh.materials[1].SetTexture(_mainTexture, card.ObverseTex);
        // _opaqueObverse.SetTexture(_mainTexture, card.ObverseTex);
        
        _opaqueObverse.mainTexture = card.ObverseTex;
        _transparentObverse.mainTexture = card.ObverseTex;

        _mesh.GetComponent<Collider>().enabled = true;

        if (_nameText is not null) _nameText.text = Card.Name;
    }

    public void AddInfluenceCardOnTop(PlayableCard influenceCard)
    {
        if (InfluenceCardOnTop is not null) throw new Exception("carta del view ya tenia influencia encima!");
        InfluenceCardOnTop = influenceCard;
    }
    public void RemoveInfluenceCardOnTop()
    {
        if (InfluenceCardOnTop is null) throw new Exception("carta del view no tenia influencia encima!");
        InfluenceCardOnTop = null;
    }


    private void SetTransparency(bool on)
    {
        _mesh.materials = on ? new[] { _transparentReverse, _transparentObverse } : new[] { _opaqueReverse, _opaqueObverse };
    }
    
}