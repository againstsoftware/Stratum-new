using System;
using UnityEngine.InputSystem;
using UnityEngine;

public class InputHandler
{
    private InputActionAsset _inputActions;

    public event Action<float> Scroll;
    public event Action<Vector2> PointerPosition;
    public event Action Press;
    public event Action TapPress;

    private Vector2 _pointerPosition;
    private IInteractionSystem _interactionSystem;

    private InteractableInput _lastTapped;

    public InputHandler(IInteractionSystem interactionSystem, InputActionAsset inputActions)
    {
        _interactionSystem = interactionSystem;
        _inputActions = inputActions;
        _inputActions.FindAction("Scroll").performed += OnScroll;
        _inputActions.FindAction("PointerPosition").performed += OnPointerPositionChanged;
        _inputActions.FindAction("Tap").performed += OnTap;
        _inputActions.FindAction("PointerPress").performed += OnPress;
        _inputActions.FindAction("PointerPress").performed += OnPress;
        _inputActions.FindAction("TapPress").performed += OnTapPress;


    }

    ~InputHandler()
    {
        _inputActions.FindAction("Scroll").performed -= OnScroll;
        _inputActions.FindAction("PointerPosition").performed -= OnPointerPositionChanged;
        _inputActions.FindAction("Tap").performed -= OnTap;
        _inputActions.FindAction("PointerPress").performed -= OnPress;

    }

    private void OnTap(InputAction.CallbackContext ctx)
    {
        Ray ray = _interactionSystem.Camera.ScreenPointToRay(_pointerPosition);
        var hit = Physics.Raycast(ray, out var hitInfo, float.MaxValue, _interactionSystem.InteractablesLayer);

        if (!hit || hitInfo.collider is null)
        {
            if(_lastTapped is not null) _lastTapped.OnPointerExit(null);
            return;
        }

        var newTapped = hitInfo.collider.GetComponentInParent<InteractableInput>();
        if(_lastTapped is not null && newTapped == _lastTapped) _lastTapped.OnPointerExit(null);
        newTapped.OnPointerEnter(null);
        newTapped.OnTap();
        _lastTapped = newTapped;
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        Scroll?.Invoke(ctx.ReadValue<Vector2>().y);
    }

    private void OnPointerPositionChanged(InputAction.CallbackContext ctx)
    {
        _pointerPosition = ctx.ReadValue<Vector2>();
        PointerPosition?.Invoke(_pointerPosition);
    }

    private void OnPress(InputAction.CallbackContext ctx)
    {
        Press?.Invoke();
    }

    private void OnTapPress(InputAction.CallbackContext ctx)
    {
        TapPress?.Invoke();
    }
}
