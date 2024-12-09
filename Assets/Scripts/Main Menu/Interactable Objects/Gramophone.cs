using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using UnityEngine.InputSystem;


public class Gramophone : AInteractableObject
{
    [SerializeField] private Collider _Language, _Graphics, _Volume;
    [SerializeField] private RenderPipelineAsset[] _qualityLevels;
    [SerializeField] private GramophoneAnimations _animations;
    //[SerializeField] private InteractionSystemMenu _interactionSystemMenu;

    private void Start()
    {
        cameraOffset = new Vector3(0f,3.5f,-4f);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        // _Language
        if (_isEnabled && (eventData.pointerCurrentRaycast.gameObject.GetComponent<Collider>() == _Language))
        {
            StartCoroutine(ToggleLanguage());
        }

        // gráficos
        if (_isEnabled && (eventData.pointerCurrentRaycast.gameObject.GetComponent<Collider>() == _Graphics))
        {
            ToggleGraphicsQuality();
        }

        // volumen
        if (_isEnabled && (eventData.pointerCurrentRaycast.gameObject.GetComponent<Collider>() == _Volume))
        {
            AudioVolume();
        }
    }
    
    
    private IEnumerator ToggleLanguage()
    {
        LocalizationGod.ToggleLanguage();

        PlayerPrefs.SetString(GamePrefs.LanguagePrefKey, LocalizationGod.Spanish ? "es" : "en");
        PlayerPrefs.Save();
        
        _interactionSystemMenu.TriggerChangedLanguage();

        _animations.VinylAnim();

        // para no liarla mientras se mueve el disco
        _interactionSystemMenu.GetComponent<PlayerInput>().enabled = false;
        while(!_animations.vinylEnd)
        {
            yield return null;
        }
        _interactionSystemMenu.GetComponent<PlayerInput>().enabled = true;


    }
    private void ToggleGraphicsQuality()
    {
        int currentQuality = QualitySettings.GetQualityLevel();

        int newQuality = (currentQuality + 1) % 2;
        QualitySettings.SetQualityLevel(newQuality);

        QualitySettings.renderPipeline = _qualityLevels[newQuality];

        PlayerPrefs.SetInt(GamePrefs.QualityPrefKey, newQuality);
        PlayerPrefs.Save();

        _animations.HandleAnim();

    }

    private void AudioVolume()
    {
        AudioSource audioSource = MusicManager.Instance.GetComponent<AudioSource>();
        audioSource.volume = (audioSource.volume + 0.2f > 1.0f) ? 0f : audioSource.volume + 0.2f;
        PlayerPrefs.SetFloat(GamePrefs.AudioPrefKey, audioSource.volume);
        PlayerPrefs.Save();

        _animations.VolumeAnim();
    }

    public override void UpdateText()
    {
        //
    }
}
