using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private GameConfig _config;
    [SerializeField] private Deck[] _decks;
    
    public enum GameMode { Match, Tutorial, Test }

    [SerializeField] private GameMode _gameMode;
    private static readonly int _globalLightPos = Shader.PropertyToID("_GlobalLightPos");

    private void Awake()
    {
        LocalizationGod.Init();
        
        ServiceLocator.Register<IRNG>(new RNG());
        
        var gameModel = new GameModel(_config, _decks);
        
        ServiceLocator.Register<IModel>(gameModel);
        
        ServiceLocator.Register<IInteractionSystem>(FindAnyObjectByType<InteractionManager>());
        
        ServiceLocator.Register<IRulesSystem>(FindAnyObjectByType<RulesManager>()); 
        
        ServiceLocator.Register<IView>(FindAnyObjectByType<ViewManager>());


        switch (_gameMode)
        {
            case GameMode.Match:
                ServiceLocator.Register<ITurnSystem>(FindAnyObjectByType<TurnManager>());
                ServiceLocator.Register<ICommunicationSystem>(FindAnyObjectByType<GameNetwork>());
                break;
            
            case GameMode.Test:
                ServiceLocator.Register<ITurnSystem>(FindAnyObjectByType<TurnManager>());
                ServiceLocator.Register<ICommunicationSystem>(FindAnyObjectByType<TestModeCommunications>());
                break;
            
            case GameMode.Tutorial:
                ServiceLocator.Register<ITurnSystem>(FindAnyObjectByType<TutorialManager>());
                ServiceLocator.Register<ICommunicationSystem>(FindAnyObjectByType<TutorialManager>());
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }

        var executor = new EffectExecutor();
        ServiceLocator.Register<IExecutor>(executor);
        executor.IsOnTutorial = _gameMode is GameMode.Tutorial;

        var lightPos = FindAnyObjectByType<Light>().transform.position;
        Shader.SetGlobalVector(_globalLightPos, lightPos);

    }

    private IEnumerator Start()
    {
        Debug.Log("Esperando a que se carguen las tablas de localizacion...");
        yield return null;
        yield return new WaitUntil(() => LocalizationGod.IsInitialized);
        
        
        ServiceLocator.Get<ITurnSystem>().StartGame();
    }

    // private void Update()
    // {
    //     var lightPos = FindAnyObjectByType<Light>().transform.position;
    //     Shader.SetGlobalVector(_globalLightPos, lightPos);
    //
    // }

    private void OnDestroy()
    {
        ServiceLocator.Clear();
    }
}