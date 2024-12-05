
using System;
using UnityEngine;

public class TurnMarker : MonoBehaviour
{
    [SerializeField] private Transform _arrowPivot;
    [SerializeField] private float _rotationDuration = .25f;
    private Quaternion _defaultRot;
    private float _defaultAngle;
    private bool _isRotating;
    private float _t;
    private Quaternion _initialRot, _targetRot;

    private void Awake()
    {
        _defaultRot = transform.rotation;
        _defaultAngle = _defaultRot.eulerAngles.y;
    }

    private void Start()
    {
        GetComponentInParent<PlayerOrientedCenter>().OnRotateToPlayer += OnRotateToPlayer;
        ServiceLocator.Get<ITurnSystem>().OnTurnChanged += OnTurnChanged;
    }

    private void Update()
    {
        if (!_isRotating) return;

        _t += Time.deltaTime / _rotationDuration;
        _arrowPivot.rotation = Quaternion.Lerp(_initialRot, _targetRot, _t);

        if (_t >= 1f)
        {
            _isRotating = false;
            _arrowPivot.rotation = _targetRot;
        }
    }

    private void OnRotateToPlayer()
    {
        transform.rotation = _defaultRot;
    }

    private void OnTurnChanged(PlayerCharacter player)
    {
        if (player is PlayerCharacter.None) return;

        _initialRot = _arrowPivot.rotation;
        _targetRot = Quaternion.AngleAxis(GetAngle(player), Vector3.up);
        _t = 0f;
        _isRotating = true;
    }

    private float GetAngle(PlayerCharacter player) => player switch
    {
        PlayerCharacter.Sagitario => _defaultAngle,
        PlayerCharacter.Fungaloth => _defaultAngle - 90f,
        PlayerCharacter.Ygdra => _defaultAngle - 180,
        PlayerCharacter.Overlord => _defaultAngle - 270,
        PlayerCharacter.None => _defaultAngle - 270,
        _ => throw new ArgumentOutOfRangeException(nameof(player), player, null)
    };
}
