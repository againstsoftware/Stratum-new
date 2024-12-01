using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TutorialRulebook : MonoBehaviour
{
    [SerializeField] private float _minSpeed, _maxSpeed;
    [SerializeField] private float _roamArea, _roamSpeed;

    private Rulebook _rulebook;

    private Animator _animator;
    private static readonly int _speed = Animator.StringToHash("Speed");
    private static readonly int _yap = Animator.StringToHash("yap");

    private bool _init;
    private Vector3 _startPos;
    private Vector3 _destination;
    private Vector3 _velocity;
    private float _smoothTime;
    private Camera _cam;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        var comms = ServiceLocator.Get<ICommunicationSystem>();
        comms.OnLocalPlayerChange += SetLocalPlayer;
        SetLocalPlayer(comms.LocalPlayer, comms.Camera);
    }

    private void OnDestroy()
    {
        ServiceLocator.Get<ICommunicationSystem>().OnLocalPlayerChange += SetLocalPlayer;
    }

    private void Update()
    {
        if (!_init) return;
        transform .LookAt(_cam.transform);
        // transform.position =
        //     Vector3.MoveTowards(transform.position, _destination, _roamSpeed * Time.deltaTime);
        
        transform.position = Vector3.SmoothDamp(transform.position, _destination, ref _velocity, _smoothTime);

        
        if(Vector3.Distance(transform.position, _destination) < .01f)
            PickNewDestinationToRoam();
    }

    private void PickNewDestinationToRoam()
    {
        _destination = _startPos + Random.insideUnitSphere * _roamArea;
        _smoothTime = Vector3.Distance(transform.position, _destination) / _roamSpeed;
    }

    public void RandomizeSpeed()
    {
        _animator.SetFloat(_speed, Random.Range(_minSpeed, _maxSpeed));
    }

    public void DisplayTutorialDialogue(TutorialDialogue dialogue, Action onFinished)
    {
        _animator.SetBool(_yap, true);
        _rulebook.DisplayDialogue(dialogue, () => _animator.SetBool(_yap, false), onFinished);
    }

    private void SetLocalPlayer(PlayerCharacter localPlayer, Camera cam)
    {
        if (localPlayer is PlayerCharacter.None) return;

        _rulebook = ServiceLocator.Get<IView>().GetViewPlayer(localPlayer).GetComponentInChildren<Rulebook>();

        //muy cutre perdon pero son las 12:04 y me quiero sobar

        var angle = localPlayer switch
        {
            PlayerCharacter.Sagitario => 0f,
            PlayerCharacter.Fungaloth => -90f,
            PlayerCharacter.Ygdra => 180f,
            PlayerCharacter.Overlord => 90f,
            _ => throw new ArgumentOutOfRangeException()
        };
        transform.rotation = Quaternion.identity;
        transform.RotateAround(Vector3.zero, Vector3.up, angle);
        
        _cam = cam;
        _startPos = transform.position;
        _init = true;
        PickNewDestinationToRoam();
    }

}