using System;
using System.Collections;
using UnityEngine;

public class TerritoryReceiver : MonoBehaviour, IActionReceiver
{
    [field: SerializeField] public PlayerCharacter Owner { get; private set; }
    [field: SerializeField] public SlotReceiver[] Slots { get; private set; }
    [field: SerializeField] public Transform SnapTransform { get; private set; }
    public Transform GetSnapTransform(PlayerCharacter _) => SnapTransform;

    public bool IsDropEnabled { get; private set; } = true;
    public bool CanInteractWithoutOwnership => true;

    public bool HasConstruction { get; private set; }
    [SerializeField] private Material _highlightedMaterial;
    [SerializeField] private float _validSelectedIntensity = 2f;

    [SerializeField] private GameObject _construction;
    [SerializeField] private GameObject _preview;
    [SerializeField] private ParticleSystem _fire;

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
        _construction.transform.Find("Destroy Smoke").TryGetComponent<ParticleSystem>(out var smoke);
        if(smoke) smoke.Play();
        HasConstruction = true;
        OnChoosingDeselect();
    }

    public void DestroyConstruction(bool isIvy)
    {
        _construction.SetActive(false);
        _construction.transform.Find("Destroy Smoke").TryGetComponent<ParticleSystem>(out var smoke);
        if(smoke) smoke.Play();
        if (isIvy)
        {
            _construction.transform.Find("Construction Vine").TryGetComponent<ParticleSystem>(out var vine);
            if(vine) vine.Play();
        }
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

    public void SetOnFire(Action callback)
    {
        StartCoroutine(PlayFire(callback));
    }

    private IEnumerator PlayFire(Action callback)
    {
        if(_fire) _fire.Play();
        yield return null;
        callback?.Invoke();
    }


    public Receiver GetReceiverStruct(ValidDropLocation actionDropLocation) =>
        new(actionDropLocation, Owner, -1, -1);
}