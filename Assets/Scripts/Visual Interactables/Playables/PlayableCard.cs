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

    [SerializeField] private float _validSelectedIntensity;
    [SerializeField] private GameObject _preview;

    [SerializeField] private DestroyableCard _destroyableCard;

    private float _startZ;
    private bool _canInteractWithoutOwnership = false;

    private Transform _hand;

    private Material _opaqueObverse, _opaqueReverse;

    private Color _defaultColor;

    private static float _persistentVerticalOffset = 0f;
    private static float _persistentIncrement =  0.0007f;
    
    protected override void Awake()
    {
        base.Awake();
        
        _hand = transform.parent;
        _opaqueReverse = _mesh.materials[0];
        _opaqueObverse = _mesh.materials[1];

        _transparentObverse = Instantiate(_transparentObverse); //reverso no hace falta pq no lo cambiamos

        _defaultColor = _opaqueObverse.color;
    }


    public void Play(IActionReceiver playLocation, Action onPlayedCallback, bool isEndOfAction = true, bool isIKTargeted = false)
    {
        OnCardPlayed?.Invoke(this);

        bool isAlreadyPlayed = CurrentState is not State.Playable && IsOnPlayLocation(playLocation);

        if(playLocation is not DiscardPileReceiver) SoundManager.Instance.PlaySound("PlayCard");

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
        float duration = isIKTargeted ? _playTravelDuration * 2f : _playTravelDuration;
        
        Travel(playLocation.GetSnapTransform(Owner), duration, State.Played, () =>
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

    //PARA CARTAS DE INFLUENCIA QUE SON DESTRUIDAS ANTES DE REPORTAR EL CALLBACK DE FIN DE ACCION


    private void OnPopulationPlayed(IActionReceiver playLocation)
    {
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
        CurrentState = State.Played;
        IsDropEnabled = false;
        _canInteractWithoutOwnership = true;
        // transform.parent = cardWherePlaced.transform;
        CardWherePlaced = cardWherePlaced;
        CardWherePlaced.AddInfluenceCardOnTop(this);

        var col = _mesh.GetComponent<BoxCollider>();
        var size = col.size;
        size.x *= 1.25f;
        col.size = size;
        
        //para que 2 cartas de influencia persistentes lado a lado no se overlapeen y causen tearing
        _mesh.transform.Translate(0f, _persistentVerticalOffset, 0f, Space.World);
        _persistentVerticalOffset += _persistentIncrement;
        if (_persistentVerticalOffset is <= 0f or > .007f)
            _persistentIncrement *= -1f;
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

    public void OnValidSelect()
    {
        _opaqueObverse.color = _defaultColor * _validSelectedIntensity;
        _preview.SetActive(true);
    }

    public void OnValidDeselect()
    {
        _opaqueObverse.color = _defaultColor;
        _preview.SetActive(false);
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


    public void DrawTravel(Transform target, Action callback)
    {
        InHandPosition = target.position;
        InHandRotation = target.rotation;
        Travel(target, _drawTravelDuration, State.Playable, callback);
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

        CurrentState = State.Played;
        IsDropEnabled = true;
        _canInteractWithoutOwnership = true;
        SlotWherePlaced = slot;
    }


    public void SetCard(ACard card)
    {
        Card = card;
        
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

    public void DestroyCard(Action callback)
    {
        SoundManager.Instance.PlaySound("BurnDiscard");
        _destroyableCard.StartDestroying(callback);
        CurrentState = State.Destroying;
        if(InfluenceCardOnTop is not null)
            InfluenceCardOnTop.DestroyCard(null);
    }


    private void SetTransparency(bool on)
    {
        _mesh.materials = on ? new[] { _transparentReverse, _transparentObverse } : new[] { _opaqueReverse, _opaqueObverse };
    }
    
    
    
}