using System;
using UnityEngine;

public class DiscardPileReceiver : MonoBehaviour, IActionReceiver
{
    [field:SerializeField] public PlayerCharacter Owner { get; private set; }
    [field:SerializeField] public Transform SnapTransform { get; private set; }
    public Transform GetSnapTransform(PlayerCharacter _) => SnapTransform;
    
    public bool IsDropEnabled { get; private set; } = true;
    public bool CanInteractWithoutOwnership => false;

    [SerializeField] private Material _highlightedMaterial;
    [SerializeField] private float _validSelectedIntensity = 2f;


    private Material _material;
    private Color _defaultColor;
    private static readonly int _color = Shader.PropertyToID("_Color");

    private void Awake()
    {
        _material = GetComponent<MeshRenderer>().material;
        _defaultColor = _material.color;
    }

    public void ShowDiscarded()
    {
        if(!SnapTransform.GetChild(0).gameObject.activeSelf)
            SnapTransform.GetChild(0).gameObject.SetActive(true);
    }

    public void OnDraggingSelect()
    {
        GetComponent<MeshRenderer>().material = _highlightedMaterial;
    }

    public void OnDraggingDeselect()
    {
        GetComponent<MeshRenderer>().material = _material;
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
    }

    public void OnValidDeselect()
    {
        _material.SetColor(_color, _defaultColor);
    }

    public Receiver GetReceiverStruct(ValidDropLocation actionDropLocation) => 
        new (actionDropLocation, Owner, -1, -1);
    
}
