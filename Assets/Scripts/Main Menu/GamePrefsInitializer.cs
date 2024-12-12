using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GamePrefsInitializer : MonoBehaviour
{
    [SerializeField] public RenderPipelineAsset[] _qualityLevels;
    private static readonly int _globalLightPos = Shader.PropertyToID("_GlobalLightPos");

    private void Start()
    {
        // cargar ajustes
        LoadSavedLanguage();
        LoadAudioVolume();
        
        LocalizationGod.Init();
        
        var lightPos = FindAnyObjectByType<Light>().transform.position;
        Shader.SetGlobalVector(_globalLightPos, lightPos);
    }
    private void LoadSavedLanguage()
    {
        string savedLocaleCode = PlayerPrefs.GetString(GamePrefs.LanguagePrefKey, "es");

        bool spanish = savedLocaleCode == "es";
        
        if(spanish != LocalizationGod.Spanish) LocalizationGod.ToggleLanguage();
    }
    private void LoadAudioVolume()
    {
        MusicManager.Instance.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat(GamePrefs.AudioPrefKey, 1.0f);
        MusicManager.Instance.PlayMusic("MenuTheme");

    }
}
