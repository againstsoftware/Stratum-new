using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private Image _fadeImg;

    private static readonly int _fadeIn = Animator.StringToHash("fade in");
    private static readonly int _fadeOut = Animator.StringToHash("fade out");

    private Animator _animator;

    private string _currentSceneToLoad;
    private bool _loadWithNetwork;

    private void Awake()
    {
        if (Instance is not null)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
        _animator = GetComponent<Animator>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    public void TransitionToCurrentScene(bool net = false)
    {
        TransitionToScene(SceneManager.GetActiveScene().name, net);
    }

    public void TransitionToScene(string sceneName, bool net = false)
    {
        if (!_animator.enabled) _animator.enabled = true;
        _loadWithNetwork = net;
        _currentSceneToLoad = sceneName;
        _animator.Play(_fadeOut);
    }

    public void SetFade(float opacity)
    {
        if (_animator.enabled) _animator.enabled = false;
        var color = _fadeImg.color;
        color.a = opacity;
        _fadeImg.color = color;
    }

    public void InstantSwapScene(string sceneName, bool net = false)
    {
        _currentSceneToLoad = sceneName;
        OnFadeOutEnd();
    }

    public void OnFadeOutEnd()
    {
        if (_currentSceneToLoad is null) return;

        if (_loadWithNetwork)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(_currentSceneToLoad, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(_currentSceneToLoad, LoadSceneMode.Single);
        }

        _currentSceneToLoad = null;
        _loadWithNetwork = false;
    }


    private void OnSceneLoaded(Scene scene = default, LoadSceneMode mode = default)
    {
        if (!_animator.enabled) _animator.enabled = true;
        _animator.Play(_fadeIn);
    }
}