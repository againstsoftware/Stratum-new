using System;
using System.Collections.Generic;
using UnityEngine;

public class SlotReceiver : MonoBehaviour, IActionReceiver
{
    [field:SerializeField] public PlayerCharacter Owner { get; private set; }
    [field:SerializeField] public TerritoryReceiver Territory { get; private set; }
    [field:SerializeField] public Transform SnapTransform { get; private set; }
    public Transform GetSnapTransform(PlayerCharacter _) => SnapTransform;
    [field:SerializeField] public Transform SnapTransformBottom { get; private set; }
    public bool IsDropEnabled { get; private set; } = true;
    public bool CanInteractWithoutOwnership => true;
    public int IndexOnTerritory { get; set; }

    public IReadOnlyList<PlayableCard> Cards => _cards;
    
    
    
    [SerializeField] private Vector3 _offset;
    [SerializeField] private Material _highlightedMaterial;
    [SerializeField] private float _validSelectedIntensity = 2f;
    [SerializeField] private GameObject _preview;

    private static readonly int _color = Shader.PropertyToID("_Color");

    private readonly List<PlayableCard> _cards = new();
    
    private Material _material;
    private Color _defaultColor;

    private void Awake()
    {
        _material = GetComponent<MeshRenderer>().material;
        _defaultColor = _material.color;
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
        _preview.SetActive(true);
    }

    public void OnValidDeselect()
    {
        _material.SetColor(_color, _defaultColor);
        _preview.SetActive(false);
    }

    
    public Receiver GetReceiverStruct(ValidDropLocation actionDropLocation) => 
        new (actionDropLocation, Owner, IndexOnTerritory, -1);

    
    public void AddCardOnTop(PlayableCard card)
    {
        _cards.Add(card);    
        UpdateCards();
    }

    public void AddCardAtTheBottom(PlayableCard card)
    {
        _cards.Insert(0, card);
        UpdateCards();
    }

    public void RemoveCard(PlayableCard card)
    {
        _cards.Remove(card);
        UpdateCards();
    }

    private void UpdateCards()
    {
        int i = 0;
        foreach (var c in _cards)
        {
            c.IndexOnSlot = i;
            SnapTransform.localPosition = SnapTransformBottom.localPosition + i * _offset;
            
            if (c.InfluenceCardOnTop is not null)
                c.InfluenceCardOnTop.transform.parent = c.transform;
            
            c.transform.position = SnapTransform.position;
            
            if (c.InfluenceCardOnTop is not null)
                c.InfluenceCardOnTop.transform.parent = null;

            i++;
        }
        SnapTransform.localPosition = SnapTransformBottom.localPosition + _cards.Count * _offset;
    }

}