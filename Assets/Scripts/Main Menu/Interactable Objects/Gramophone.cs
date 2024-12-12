using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using UnityEngine.InputSystem;


public class Gramophone : AInteractableObject
{
    [SerializeField] private Collider _Language, _SfxVolume, _Volume;
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

        // sfx
        if (_isEnabled && (eventData.pointerCurrentRaycast.gameObject.GetComponent<Collider>() == _SfxVolume))
        {
            SfxVolume();
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

    private void SfxVolume()
    {
        AudioSource audioSource = SoundManager.Instance.GetComponent<AudioSource>();
        audioSource.volume = (audioSource.volume + 0.25f > 0.5) ? 0f : audioSource.volume + 0.25f;

        PlayerPrefs.SetFloat(GamePrefs.SfxPrefKey, audioSource.volume);
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
