
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class TableCenter : MonoBehaviour, IActionReceiver
{
    [field:SerializeField] public Transform SnapTransform { get; private set; }

    public PlayerCharacter Owner => PlayerCharacter.None;
    public bool IsDropEnabled => true;
    public bool CanInteractWithoutOwnership => true;

    [SerializeField] private MeshRenderer _tableMesh;


    [SerializeField] private float _sagitarioRotation, _fungalothRotation, _ygdraRotation, _overlordRotation;
    [SerializeField] private Material _highlightedMaterial;
    [SerializeField] private float _validSelectedIntensity = 2f;
    [SerializeField] private GameObject _preview;
    
    
    private static readonly int _color = Shader.PropertyToID("_Color");
    private Material _material;
    private Color _defaultColor;
    private Vector3 _defaultEulers;
    
    private void Awake()
    {
        _material = _tableMesh.material;
        _defaultEulers = SnapTransform.localRotation.eulerAngles;
        _defaultColor = _material.color;
    }

    private void Start()
    {
        var comms = ServiceLocator.Get<ICommunicationSystem>(); 
        comms.OnLocalPlayerChange += RotateToPlayer;
        RotateToPlayer(comms.LocalPlayer);
    }

    public void OnDraggingSelect()
    {
        _tableMesh.material = _highlightedMaterial;
    }

    public void OnDraggingDeselect()
    {
        _tableMesh.material = _material;
    }
    
    
    public void OnChoosingSelect()
    {
        OnDraggingSelect();
    }

    public void OnChoosingDeselect()
    {
        OnDraggingDeselect();
    }
    
    public void OnValidSelect()
    {
        _material.SetColor(_color, _defaultColor * _validSelectedIntensity);
        _preview.SetActive(true);
    }

    public void OnValidDeselect()
    {
        _material.SetColor(_color, _defaultColor);
        _preview.SetActive(false);
    }

    public Receiver GetReceiverStruct(ValidDropLocation actionDropLocation)
    {
        return new Receiver(ValidDropLocation.TableCenter, PlayerCharacter.None, -1, -1);   
        throw new Exception("El centro de la mesa no es un receiver valido!! solo sirve para ensamblar la jugada!");
    }

    public Transform GetSnapTransform(PlayerCharacter character)
    {
        var newRot = _defaultEulers;
        newRot.z = GetPlayerRotation(character);

        SnapTransform.localRotation = Quaternion.Euler(newRot);
        return SnapTransform;
    }

    private void RotateToPlayer(PlayerCharacter player, Camera _ = null)
    {
        var rot = GetPlayerRotation(player);
        transform.Find("Mesh").rotation = Quaternion.AngleAxis(90f - rot, Vector3.up);
    }

    private float GetPlayerRotation(PlayerCharacter player) => player switch
    {
        PlayerCharacter.Sagitario => _sagitarioRotation,
        PlayerCharacter.Ygdra => _ygdraRotation,
        PlayerCharacter.Fungaloth => _fungalothRotation,
        PlayerCharacter.Overlord => _overlordRotation,
        // PlayerCharacter.None => _sagitarioRotation,
        _ => throw new ArgumentOutOfRangeException()
    };
}
