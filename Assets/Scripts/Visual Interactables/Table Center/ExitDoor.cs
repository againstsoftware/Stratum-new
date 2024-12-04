
using System;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    [SerializeField] private float _holdTime;
    [SerializeField] private Transform _doorHinge;
    [SerializeField] private float _maxhingeAngle = -110f;
    [SerializeField] private MeshRenderer _portalRenderer;
    
    private TableItemInteraction _interaction;
    
    private float _t;
    
    private enum State {Idle, Holding, Resetting}
    private State _state = State.Idle;
    private Vector3 _defaultEulers, _eulers;
    private Material _portalMaterial;
    private Color _defaultColor, _color;
    private static readonly int _colorHash = Shader.PropertyToID("_Color");
    private static readonly int _speedHash = Shader.PropertyToID("_Speed");

    private void Awake()
    {
        _interaction = GetComponent<TableItemInteraction>();
        _interaction.OnItemPress += OnPress;
        _interaction.OnItemRelease += OnRelease;
        _defaultEulers = _doorHinge.localRotation.eulerAngles;
        _portalMaterial = _portalRenderer.material;
        _defaultColor = _portalMaterial.GetColor(_colorHash);
        _defaultColor.a = 0f;
        _portalMaterial.SetColor(_colorHash, _defaultColor);
    }

    private void Update()
    {
        switch(_state)
        {
            case State.Idle:
                break;
            case State.Holding:
                _t += Time.deltaTime / _holdTime;
                _t = Mathf.Clamp01(_t);
                SceneTransition.Instance.SetFade(_t);
                _eulers = _defaultEulers;
                _eulers.z += _maxhingeAngle * _t;
                _doorHinge.localRotation = Quaternion.Euler(_eulers);
                _color = _defaultColor;
                _color.a = _t;
                _portalMaterial.SetColor(_colorHash, _color);
                if (_t < 1f) return;
                ServiceLocator.Get<ICommunicationSystem>().Disconnect();
                SceneTransition.Instance.InstantSwapScene("MainMenu");
                break;
            case State.Resetting:
                _t -= 3f * Time.deltaTime / _holdTime;
                _t = Mathf.Clamp01(_t);
                SceneTransition.Instance.SetFade(_t);
                _eulers = _defaultEulers;
                _eulers.z += _maxhingeAngle * _t;
                _doorHinge.localRotation = Quaternion.Euler(_eulers);
                _color = _defaultColor;
                _color.a = _t;
                _portalMaterial.SetColor(_colorHash, _color);
                if (_t > 0f) return;
                SceneTransition.Instance.SetFade(0f);
                _doorHinge.localRotation = Quaternion.Euler(_defaultEulers);
                _portalMaterial.SetColor(_colorHash, _defaultColor);
                _state = State.Idle;
                _t = 0f;
                break;
        }

        
    }

    private void OnPress()
    {
        _state = State.Holding;
        _portalMaterial.SetFloat(_speedHash, Mathf.Abs(_portalMaterial.GetFloat(_speedHash)));
    }

    private void OnRelease()
    {
        _state = State.Resetting;
        _portalMaterial.SetFloat(_speedHash, -Mathf.Abs(_portalMaterial.GetFloat(_speedHash)));

    }
}
