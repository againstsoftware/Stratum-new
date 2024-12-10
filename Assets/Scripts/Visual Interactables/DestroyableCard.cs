using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DestroyableCard : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private float _initialCutoff;
    [SerializeField] private float _endCutoff;
    [SerializeField] private float _destroyDuration;
    [SerializeField] private Material _dissolveMaterial;
    [SerializeField] private TextMeshProUGUI _cardText;

    private static readonly int _mainTex = Shader.PropertyToID("_MainTex");
    private static readonly int _cutoffHeight = Shader.PropertyToID("_CutoffHeight");
    private static readonly int _randomSeed = Shader.PropertyToID("_RandomSeed");

    private float _t = 0f;
    private Action _callback;
    private bool _isDestroying = false;
    private bool _onlyAnimation = false;

    public void DestroyAnimation() => StartDestroying(null, true);

    public void StartDestroying(Action callback, bool onlyAnimation = false)
    {
        _isDestroying = true;
        _onlyAnimation = onlyAnimation;

        _callback = callback;

        _renderer.SetMaterials(new List<Material> { _dissolveMaterial, _dissolveMaterial });


        if (TryGetComponent<PlayableCard>(out var playableCard))
            _renderer.materials[1].SetTexture(_mainTex, playableCard.Card.ObverseTex);

        _renderer.materials[0].SetFloat(_cutoffHeight, _initialCutoff);
        _renderer.materials[1].SetFloat(_cutoffHeight, _initialCutoff);

        var randomSeed = new Vector2(Random.Range(0f, 10f), Random.Range(0f, 10f));
        _renderer.materials[0].SetVector(_randomSeed, randomSeed);
        _renderer.materials[1].SetVector(_randomSeed, randomSeed);
        
    }

    private void Update()
    {
        if (!_isDestroying) return;
        _t += Time.deltaTime / _destroyDuration;
        var cutoff = Mathf.Lerp(_initialCutoff, _endCutoff, _t);
        _renderer.materials[0].SetFloat(_cutoffHeight, cutoff);
        _renderer.materials[1].SetFloat(_cutoffHeight, cutoff);

        if (_cardText is not null)
        {
            var color = _cardText.color;
            color.a = Mathf.Clamp01(1f - _t*3f);
            _cardText.color = color;
        }

        if (_t < 1f) return;

        if (!_onlyAnimation)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
            _renderer.materials[0].SetFloat(_cutoffHeight, _initialCutoff);
            _renderer.materials[1].SetFloat(_cutoffHeight, _initialCutoff);
            _t = 0f;
            _isDestroying = false;
        }

        _callback?.Invoke();
        _callback = null;
    }
}