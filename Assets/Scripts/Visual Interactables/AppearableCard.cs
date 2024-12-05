using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class AppearableCard : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private float _initialCutoff;
    [SerializeField] private float _endCutoff;
    [SerializeField] private float _appearDuration;
    [SerializeField] private Material _materializeMaterial;
    [SerializeField] private TextMeshProUGUI _cardText;
    [SerializeField] private Animator _animator;

    private static readonly int _mainTex = Shader.PropertyToID("_MainTex");
    private static readonly int _cutoffHeight = Shader.PropertyToID("_CutoffHeight");

    private float _t = 0f;
    private Action _callback;
    private bool _isMaterializing = false;

    private Material[] _defaultMaterials;

    public void AppearCard(ACard card, Action callback)
    {
        if(card is MushroomCard or MacrofungiCard)
            StartMaterializing(card, callback);
        else if (card is PopulationCard)
            StartGrowing(card, callback);
    }
    
    

    private void StartMaterializing(ACard card, Action callback)
    {
        _isMaterializing = true;

        _callback = callback;

        _defaultMaterials = _renderer.materials;

        _renderer.SetMaterials(new List<Material> { _materializeMaterial, _materializeMaterial });

        if (card is not null)
            _renderer.materials[1].SetTexture(_mainTex, card.ObverseTex);

        _renderer.materials[0].SetFloat(_cutoffHeight, _initialCutoff);
        _renderer.materials[1].SetFloat(_cutoffHeight, _initialCutoff);
    }

    private void StartGrowing(ACard card, Action callback)
    {
        _animator.enabled = true;
        _animator.Play("grow");
        _callback = callback;
    }

    public void OnGrowComplete()
    {
        _animator.enabled = false;
        _callback?.Invoke();
        _callback = null;
    }

    private void Update()
    {
        if (!_isMaterializing) return;
        _t += Time.deltaTime / _appearDuration;
        var cutoff = Mathf.Lerp(_initialCutoff, _endCutoff, _t);
        _renderer.materials[0].SetFloat(_cutoffHeight, cutoff);
        _renderer.materials[1].SetFloat(_cutoffHeight, cutoff);

        if (_cardText is not null)
        {
            var color = _cardText.color;
            color.a = Mathf.Clamp01(_t/2f);
            _cardText.color = color;
        }

        if (_t < 1f) return;


        _renderer.materials = _defaultMaterials;
        
        if (_cardText is not null)
        {
            var color = _cardText.color;
            color.a = 1f;
            _cardText.color = color;
        }
        
        _isMaterializing = false;

        _callback?.Invoke();
        _callback = null;
    }
}