using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialProgression : MonoBehaviour
{
    public static TutorialProgression Instance { get; private set; }

    [SerializeField] private ATutorialSequence[] _tutorials;
    private int _currentTutorialIndex = 0;

    private void Awake()
    {
        if (Instance is not null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    public ATutorialSequence GetTutorial() => _tutorials[_currentTutorialIndex++];


    private void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (scene.name != "Tutorial")
        {
            Destroy(gameObject);
        }
        
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
        Debug.Log("tutorial progression destruido");
    }
}