
using System;
using UnityEngine;

public class TurnMarker : MonoBehaviour
{
    public enum State { Idle, Turning, Spinning }
    public State CurrentState { get; private set; }

    
    [SerializeField] private Transform _arrowPivot;
    [SerializeField] private float _rotationDuration = .25f;
    [SerializeField] private float _spinningSpeed;
    private Quaternion _defaultRot;
    private float _defaultAngle;

    
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
        switch (CurrentState)
        {
            case State.Idle:
                break;
            
            case State.Turning:
                _t += Time.deltaTime / _rotationDuration;
                _arrowPivot.rotation = Quaternion.Lerp(_initialRot, _targetRot, _t);

                if (_t >= 1f)
                {
                    CurrentState = State.Idle;
                    _arrowPivot.rotation = _targetRot;
                }
                break;
            
            case State.Spinning:
                _arrowPivot.Rotate(Vector3.up, _spinningSpeed * Time.deltaTime);
                break;

        }
        


    }

    private void OnRotateToPlayer()
    {
        transform.rotation = _defaultRot;
    }

    public void OnTurnChanged(PlayerCharacter player)
    {
        if (player is PlayerCharacter.None)
        {
            CurrentState = State.Spinning;
            return;
        }

        CurrentState = State.Turning;
        _initialRot = _arrowPivot.rotation;
        _targetRot = Quaternion.AngleAxis(GetAngle(player), Vector3.up);
        _t = 0f;
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
