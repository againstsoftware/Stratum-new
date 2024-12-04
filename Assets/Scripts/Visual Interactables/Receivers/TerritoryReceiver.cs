
using UnityEngine;

public class TerritoryReceiver : MonoBehaviour, IActionReceiver
{
    [field:SerializeField] public PlayerCharacter Owner { get; private set; }
    [field:SerializeField] public SlotReceiver[] Slots { get; private set; }
    [field:SerializeField] public Transform SnapTransform { get; private set; }
    public Transform GetSnapTransform(PlayerCharacter _) => SnapTransform;

    public bool IsDropEnabled { get; private set; } = true;
    public bool CanInteractWithoutOwnership => true;

    public bool HasConstruction { get; private set; }
    [SerializeField] private Material _highlightedMaterial;
    [SerializeField] private float _validSelectedIntensity = 2f;

    [SerializeField] private GameObject _construction;
    [SerializeField] private GameObject _preview;
    
    private Material _material;
    private Color _defaultColor;
    private static readonly int _color = Shader.PropertyToID("_Color");
    private MeshRenderer _meshRenderer;
    private void Awake()
    {
        _meshRenderer = transform.Find("Mesh").GetComponent<MeshRenderer>();
        _material = _meshRenderer.material;
        _defaultColor = _material.color;

        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].IndexOnTerritory = i;
        }
    }

    public void BuildConstruction()
    {
        _construction.SetActive(true);
        // _construction.transform.localPosition = prefab.transform.position;
        //_construction.transform.localPosition = Vector3.zero;
        // _construction.transform.localRotation = prefab.transform.rotation;
        HasConstruction = true;
        OnChoosingDeselect();
    }

    public void DestroyConstruction()
    {
        _construction.SetActive(false);
        HasConstruction = false;
    }

    public void OnDraggingSelect()
    {
        _meshRenderer.material = _highlightedMaterial;
    }

    public void OnDraggingDeselect()
    {
        _meshRenderer.material = _material;
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
        new (actionDropLocation, Owner, -1, -1);
}
