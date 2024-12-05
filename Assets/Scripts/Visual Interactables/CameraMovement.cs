using UnityEngine;
using System;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float _transitionDuration = 1f;
    [SerializeField] private Transform _defaultTransform;
    [SerializeField] private Transform _overviewTransform;

    private enum CameraState
    {
        Default,
        Overview,
        Transitioning
    }

    private CameraState _currentState = CameraState.Default;
    private float _transitionProgress = 0f;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private Action _onTransitionCompleteCallback;

    private Vector3 _handDefaultPosition;

    private void Awake()
    {
        _defaultTransform.SetParent(null);
        _overviewTransform.SetParent(null);
    }

    private void Update()
    {
        UpdateCameraTransition();
    }

    private void UpdateCameraTransition()
    {
        if (_currentState != CameraState.Transitioning) return;

        // Use Mathf.Clamp01 to prevent potential floating-point precision issues
        _transitionProgress = Mathf.Clamp01(_transitionProgress + Time.deltaTime / _transitionDuration);

        // Use more precise interpolation methods
        transform.position = Vector3.Lerp(_startPosition, _targetPosition, GetEaseInOutQuad(_transitionProgress));
        transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, GetEaseInOutQuad(_transitionProgress));
        

        if (_transitionProgress >= 1f)
        {
            CompleteTransition();
        }
    }

    // Smoother easing function to improve transition feel
    private float GetEaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }

    private void CompleteTransition()
    {
        _currentState = (_targetPosition == _overviewTransform.position) 
            ? CameraState.Overview 
            : CameraState.Default;

        _onTransitionCompleteCallback?.Invoke();
        _onTransitionCompleteCallback = null;
    }

    public void MoveCameraOnScroll(float ScrollDelta)
    {
        // Use epsilon to handle small floating-point inaccuracies
        if (Mathf.Abs(ScrollDelta) < float.Epsilon) return;

        if (ScrollDelta > 0 && _currentState != CameraState.Overview)
        {
            ChangeToOverview();
        }
        else if (ScrollDelta < 0 && _currentState != CameraState.Default)
        {
            ChangeToDefault();
        }
    }

    public void ChangeToOverview(Action Callback = null)
    {
        StartTransition(_overviewTransform.position, _overviewTransform.rotation, Callback);
    }

    public void ChangeToDefault(Action Callback = null)
    {
        StartTransition(_defaultTransform.position, _defaultTransform.rotation, Callback);
    }

    private void StartTransition(Vector3 TargetPos, Quaternion TargetRot, Action Callback)
    {
        // If already transitioning, capture current interpolated position as new start
        if (_currentState == CameraState.Transitioning)
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }
        else
        {
            _startPosition = _currentState == CameraState.Default 
                ? _defaultTransform.position 
                : _overviewTransform.position;
            _startRotation = _currentState == CameraState.Default 
                ? _defaultTransform.rotation 
                : _overviewTransform.rotation;
        }

        // Set new target and reset progress
        _targetPosition = TargetPos;
        _targetRotation = TargetRot;
        _transitionProgress = 0f;
        _currentState = CameraState.Transitioning;
        
        // Set callback
        _onTransitionCompleteCallback = Callback;
    }

    public void SnapToState(bool IsOverview)
    {
        Transform targetTransform = IsOverview ? _overviewTransform : _defaultTransform;
        
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        
        _currentState = IsOverview ? CameraState.Overview : CameraState.Default;
    }
}