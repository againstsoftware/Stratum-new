using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InteractionManager : MonoBehaviour, IInteractionSystem
{
    public IInteractable SelectedInteractable { get; private set; }
    public IActionReceiver SelectedDropLocation { get; private set; }
    public IInteractionSystem.State CurrentState { get; private set; } = IInteractionSystem.State.Waiting;
    public InputHandler Input { get; private set; }

    public Camera Camera { get; private set; }
    [field: SerializeField] public LayerMask InteractablesLayer { get; private set; }

    public IReadOnlyList<IActionReceiver> CurrentActionReceivers => ActionAssembler.ActionReceivers;
    public APlayableItem CurrentActionPlayableItem => ActionAssembler.PlayableItem;

    public PlayerCharacter LocalPlayer { get; private set; }

    public bool InputEnabled => !_inputDisabled;
    
    [SerializeField] private InputActionAsset _inputActions;

    [SerializeField] private float _itemCamOffsetOnDrag;
    [SerializeField] private float _dropLocationCheckFrequency;

    [SerializeField] private float _dragCardHeight = .2f;
    [SerializeField] private float _dragCardSpeed = .2f;
    [SerializeField] private float _dragEnableTime = .5f;
    [SerializeField] private GameConfig _config;
    

    private InputAction _pointerPosAction;

    private CameraMovement _cameraMovement;
    private Rulebook _rulebook;

    private Transform _dragItemTransform;

    private Vector3 _screenPointerPosition;

    // private Vector3 _screenOffsetOnDrag;
    private bool _isSelectedRulebookOpener;
    private APlayableItem _draggingItem;
    private float _dropLocationCheckTimer, _dropLocationCheckPeriod;
    private readonly HashSet<IActionReceiver> _selectedReceivers = new();
    private IActionReceiver _selectedReceiver;

    private IActionReceiver[] _allReceivers;

    private int _actionsLeft;

    // private float _draggingTimer;
    private bool _canDrag = true;

    private bool _inputDisabled = false;

    #region Callbacks

    private void Awake()
    {
        _dropLocationCheckPeriod = 1f / _dropLocationCheckFrequency;

        Input = new(this, _inputActions);
        Input.PointerPosition += OnPointerPositionChanged;
        Input.Scroll += OnScroll;
    }


    private void Start()
    {
        ServiceLocator.Get<ITurnSystem>().OnTurnChanged += OnTurnChanged;
        ServiceLocator.Get<ITurnSystem>().OnActionEnded += OnActionEnded;
        var comms = ServiceLocator.Get<ICommunicationSystem>();
        comms.OnLocalPlayerChange += SetLocalPlayer;
        SetLocalPlayer(comms.LocalPlayer, comms.Camera);

        var allComponents = FindObjectsOfType<Component>();

        _allReceivers = allComponents
            .OfType<IActionReceiver>()
            .ToArray();
    }


    private void OnDisable()
    {
        Input.PointerPosition -= OnPointerPositionChanged;
        var ts = ServiceLocator.Get<ITurnSystem>();
        if (ts is null) return;
        ts.OnTurnChanged -= OnTurnChanged;
        ts.OnActionEnded -= OnActionEnded;
        ServiceLocator.Get<ICommunicationSystem>().OnLocalPlayerChange -= SetLocalPlayer;
    }


    private void Update()
    {
        switch (CurrentState)
        {
            case IInteractionSystem.State.Waiting:
                break;

            case IInteractionSystem.State.Idle:
                break;

            case IInteractionSystem.State.Dragging:
                var newPos = Camera.ScreenToWorldPoint(
                    new Vector3(_screenPointerPosition.x, _screenPointerPosition.y, _itemCamOffsetOnDrag));
                newPos.y = Mathf.Max(_dragCardHeight, newPos.y);
                // _dragItemTransform.position = newPos;
                _dragItemTransform.position = Vector3.MoveTowards(_dragItemTransform.position, newPos,
                    _dragCardSpeed * Time.deltaTime);

                // _draggingTimer += Time.deltaTime;
                // if (_draggingTimer < _dragEnableTime) return;
                //
                _dropLocationCheckTimer += Time.deltaTime;
                if (_dropLocationCheckTimer < _dropLocationCheckPeriod) return;
                
                _dropLocationCheckTimer = 0f;
                CheckDropLocations();
                break;

            case IInteractionSystem.State.Choosing:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnPointerPositionChanged(Vector2 pointerPos)
    {
        _screenPointerPosition = pointerPos;
    }

    private void OnScroll(float scroll)
    {
        if (CurrentState is IInteractionSystem.State.Dragging || scroll == 0f || _inputDisabled) return;
        _cameraMovement.MoveCameraOnScroll(scroll);
    }

    #endregion


    public void SelectInteractable(IInteractable item)
    {
        if (CurrentState is IInteractionSystem.State.Dragging || _inputDisabled) return;
        if (item is null)
        {
            throw new Exception("select called with null item!");
        }

        if (!item.CanInteractWithoutOwnership && item.Owner != LocalPlayer) return;
        var old = SelectedInteractable;
        if (old is not null) DeselectInteractable(old);
        SelectedInteractable = item;
        item.OnSelect();
        if (item is IRulebookEntry entry)
        {
            _isSelectedRulebookOpener = true;
            _rulebook.ShowRulebookEntry(entry);
        }
    }

    public void DeselectInteractable(IInteractable item)
    {
        if (CurrentState is IInteractionSystem.State.Dragging || SelectedInteractable != item) return;

        if (SelectedInteractable is not null) SelectedInteractable.OnDeselect();
        SelectedInteractable = null;
        if (_isSelectedRulebookOpener)
        {
            _isSelectedRulebookOpener = false;
            _rulebook.HideRulebook();
        }
    }

    public void DragPlayableItem(APlayableItem item)
    {
        if (!_canDrag) return;
        if (CurrentState is not IInteractionSystem.State.Idle)
        {
            Debug.Log($"el IM no esta en idle, esta en {CurrentState}");
            //
            // if (_draggingItem is not null)
            // {
            //     CurrentState = IInteractionSystem.State.Idle;
            //     _draggingItem.OnDragCancel();
            //     if (!_draggingItem.OnlyVisibleOnOverview) _cameraMovement.ChangeToDefault();
            //     _draggingItem = null;
            //     return;
            // }
            
            return;
            
        }

        if (item.CurrentState is not APlayableItem.State.Playable)
        {
            Debug.Log($"el obj no esta en playable, esta en {item.CurrentState}");
            return;
        }

        if (item.Owner != LocalPlayer)
        {
            Debug.Log($"el objeto no es del localplayer({LocalPlayer}) sino de {item.Owner}");
            return;
        }

        if (item != SelectedInteractable as APlayableItem)
        {
            Debug.Log("drag called with non selected item!");
            return;
        }

        _canDrag = false;
        Invoke(nameof(ResetCanDrag), _dragEnableTime);
        
        _draggingItem = item;
        item.OnDrag();
        CurrentState = IInteractionSystem.State.Dragging;
        // _draggingTimer = 0f;
        _dragItemTransform = item.transform;
        // _screenOffsetOnDrag = _cam.WorldToScreenPoint(_dragItemTransform.position) - _screenPointerPosition;
        // _screenOffsetOnDrag.z = _itemCamOffsetOnDrag;
        if (item is PlayableCard)
            _dragItemTransform.rotation = Quaternion.LookRotation(Vector3.down, _dragItemTransform.forward);

        if (!item.OnlyVisibleOnOverview) _cameraMovement.ChangeToOverview();

        if (_isSelectedRulebookOpener)
        {
            _isSelectedRulebookOpener = false;
            _rulebook.HideRulebook();
        }
        
        //ahora vamos a coger a todos los receivers de la escena y pasarlos por el isvalid del action item para 
        //si son validos medio highlightearlos par que el jugador sepa donde jugar
        
        //esto si el juego va lento hay que optimizarlo
        var allComponents = FindObjectsOfType<Component>();
        _allReceivers = allComponents
            .OfType<IActionReceiver>()
            .ToArray();
        
        foreach (var receiver in _allReceivers)
        {
            if(ActionAssembler.CheckFirstReceiver(_draggingItem, receiver))
                receiver.OnValidSelect();
        }
        
    }

    private void ResetCanDrag() => _canDrag = true;

    public void DropPlayableItem(APlayableItem item)
    {
        if (CurrentState is not IInteractionSystem.State.Dragging) return;
        if (item.CurrentState is not APlayableItem.State.Dragging) return;
        if (item != SelectedInteractable as APlayableItem)
        {
            // throw new Exception("drop called with non selected item!");
            return;
        }
        
        foreach(var receiver in _allReceivers) receiver.OnValidDeselect();


        var dropLocation = SelectedDropLocation;
        DeselectInteractable(SelectedInteractable);

        if (SelectedDropLocation is null)
        {
            CurrentState = IInteractionSystem.State.Idle;
            item.OnDragCancel();
            if (!item.OnlyVisibleOnOverview) _cameraMovement.ChangeToDefault();
            return;
        }

        SelectedDropLocation.OnDraggingDeselect();
        SelectedDropLocation = null;
        

        switch (ActionAssembler.TryAssembleAction(item, dropLocation, out string feedbackKey))
        {
            case ActionAssembler.AssemblyState.Failed:
                CurrentState = IInteractionSystem.State.Idle;
                item.OnDragCancel();
                if (!item.OnlyVisibleOnOverview) _cameraMovement.ChangeToDefault();
                
                _rulebook.DisplayDialogue(LocalizationGod.GetLocalized("Feedback", feedbackKey), null, null);

                break;

            case ActionAssembler.AssemblyState.Ongoing:
                CurrentState = IInteractionSystem.State.Choosing;
                _selectedReceivers.Clear();
                _selectedReceivers.Add(dropLocation);
                item.OnDrop(dropLocation);
                break;

            case ActionAssembler.AssemblyState.Completed:
                CurrentState = IInteractionSystem.State.Waiting;
                Debug.Log("accion ensamblada!!!");
                item.OnDrop(dropLocation);
                break;
        }
        
    }


    public void ClickReceiver(IActionReceiver receiver)
    {
        if (CurrentState is not IInteractionSystem.State.Choosing) return;
        if (!receiver.CanInteractWithoutOwnership) return;
        if (_selectedReceivers.Contains(receiver)) return;

        switch (ActionAssembler.AddReceiver(receiver, out string feedbackKey))
        {
            case ActionAssembler.AssemblyState.Failed:
                CurrentState = IInteractionSystem.State.Idle;
                _draggingItem.OnDragCancel();
                if (!_draggingItem.OnlyVisibleOnOverview) _cameraMovement.ChangeToDefault();
                if (_selectedReceiver is not null) _selectedReceiver.OnChoosingDeselect();
                foreach(var r in _selectedReceivers) r.OnChoosingDeselect();
                _rulebook.DisplayDialogue(LocalizationGod.GetLocalized("Feedback", feedbackKey), null, null);
                break;

            case ActionAssembler.AssemblyState.Ongoing:
                _selectedReceivers.Add(receiver);
                break;

            case ActionAssembler.AssemblyState.Completed:
                CurrentState = IInteractionSystem.State.Waiting;
                Debug.Log("accion ensamblada!!!");
                if (_selectedReceiver is not null) _selectedReceiver.OnChoosingDeselect();
                foreach(var r in _selectedReceivers) r.OnChoosingDeselect();
                break;
        }
    }

    public void SelectReceiver(IActionReceiver receiver)
    {
        if (CurrentState is not IInteractionSystem.State.Choosing) return;
        if (!receiver.CanInteractWithoutOwnership) return;
        if (_selectedReceivers.Contains(receiver)) return;
        if (_selectedReceiver is not null && !_selectedReceivers.Contains(_selectedReceiver)) 
            _selectedReceiver.OnChoosingDeselect();
        _selectedReceiver = receiver;
        receiver.OnChoosingSelect();
    }

    public void DeselectReceiver(IActionReceiver receiver)
    {
        if (CurrentState is not IInteractionSystem.State.Choosing) return;
        if (_selectedReceivers.Contains(receiver)) return;

        receiver.OnChoosingDeselect();
        _selectedReceiver = null;
    }


    public void Disable()
    {
        CurrentState = IInteractionSystem.State.Waiting;
    }

    public void Enable()
    {
        CurrentState = IInteractionSystem.State.Idle;
    }

    public void DisableInput()
    {
        _inputDisabled = true;
    }

    public void EnableInput() => _inputDisabled = false;


    private void CheckDropLocations()
    {
        _draggingItem.SetColliderActive(false);
        Ray ray = Camera.ScreenPointToRay(_screenPointerPosition);
        var hit = Physics.Raycast(ray, out var hitInfo, float.MaxValue, InteractablesLayer);
        _draggingItem.SetColliderActive(true);
        if (!hit || hitInfo.collider is null)
        {
            if (SelectedDropLocation is null) return;
            SelectedDropLocation.OnDraggingDeselect();
            SelectedDropLocation = null;
            return;
        }

        var newDropLocation = hitInfo.collider.GetComponentInParent<IActionReceiver>();

        if (newDropLocation == SelectedDropLocation) return;
        if (SelectedDropLocation is not null) SelectedDropLocation.OnDraggingDeselect();

        if (newDropLocation is null)
        {
            return;
            //no hace falta (?) porque pasa con los tokens que estan en la layer pero no son receivers
            //throw new Exception($"drop location no tiene iactionreceiver! : {hitInfo.collider.name}");
        }

        if (!newDropLocation.IsDropEnabled ||
            (!newDropLocation.CanInteractWithoutOwnership && newDropLocation.Owner != _draggingItem.Owner))
            return;

        SelectedDropLocation = newDropLocation;

        if (!SelectedDropLocation.IsDropEnabled)
        {
            SelectedDropLocation = null;
            return;
        }

        SelectedDropLocation.OnDraggingSelect();
    }

    private void OnActionEnded(PlayerCharacter onTurn)
    {
        if (onTurn == LocalPlayer) TryStartAction();
    }

    private void TryStartAction()

    {
        if (_actionsLeft == 0)
        {
            // Debug.Log("no actions left in IM");
            CurrentState = IInteractionSystem.State.Waiting;
            return;
        }

        // Debug.Log("starting next action");

        _actionsLeft--;
        CurrentState = IInteractionSystem.State.Idle;
    }

    private void OnTurnChanged(PlayerCharacter onTurn)
    {
        if (onTurn != LocalPlayer)
        {
            CurrentState = IInteractionSystem.State.Waiting;
            return;
        }

        _actionsLeft = _config.ActionsPerTurn;
        TryStartAction();
    }


    private void SetLocalPlayer(PlayerCharacter character, Camera cam)
    {
        LocalPlayer = character;
        Camera = cam;
        if (Camera is null || LocalPlayer is PlayerCharacter.None) return;
        
        _cameraMovement = Camera.GetComponent<CameraMovement>();
        _rulebook = Camera.GetComponentInChildren<Rulebook>();
    }
}