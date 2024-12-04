
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour, ITurnSystem, ICommunicationSystem
{
    public PlayerCharacter PlayerOnTurn { get; private set; }
    public bool IsAuthority => true;
    public bool IsRNGSynced => true;

    public PlayerCharacter LocalPlayer { get; private set; } = PlayerCharacter.None;
    public Camera Camera { get; private set; }
    
    public event Action<PlayerCharacter> OnTurnChanged;
    public event Action<PlayerCharacter> OnActionEnded;
    
    public event Action OnGameStart;
    
    public event Action<PlayerCharacter, Camera> OnLocalPlayerChange;

    public bool CanTriggerRepeat => _isCurrentPlayerAction;

    [SerializeField] private TutorialRulebook _tutorialRulebook;
    [SerializeField] private float _delayBetweenElements;

    private Queue<ITutorialElement> _tutorialElements;
    private ATutorialSequence _tutorialSequence;
    private bool _isCurrentPlayerAction;

    private List<TutorialDialogue[]> _dialogueBatches;

    private int _currentBatchIndex = -1;
    private bool _wasLastDialogue = false;
    private bool _isRepeating = false;

    private void Start()
    {
        InitTutorialElements();
        
        SetLocalPlayer();

        PlayerOnTurn = PlayerCharacter.None;
    }
    
    public void StartGame()
    {

        OnGameStart?.Invoke();
        
    }
    
    public void SendActionToAuthority(PlayerAction action)
    {
        ServiceLocator.Get<IExecutor>().ExecutePlayerActionEffects(action);
    }
    
    public void Disconnect() => Debug.Log("POV: te desconectaste.");

    public void EndAction()
    {
        if (_isCurrentPlayerAction)
        {
            _isCurrentPlayerAction = false;
            Invoke(nameof(ExecuteNextTutorialElement), .01f);
        }
        else
        {
            Invoke(nameof(ExecuteNextTutorialElement), _delayBetweenElements);
        }
    }

    private void SetLocalPlayer()
    {
        LocalPlayer = _tutorialSequence.LocalPlayer;
        foreach (PlayerCharacter character in Enum.GetValues(typeof(PlayerCharacter)))
        {
            if (character is PlayerCharacter.None) continue;

            var viewPlayer = ServiceLocator.Get<IView>().GetViewPlayer(character);
            var cam = viewPlayer.MainCamera;
            
            if (character != LocalPlayer)
            {
                Destroy(cam.gameObject);
                Destroy(viewPlayer.UICamera.gameObject);
            }
            else
            {
                viewPlayer.IsLocalPlayer = true;

                Camera = cam;
                OnLocalPlayerChange?.Invoke(LocalPlayer, cam);
            }
        }
    }

    private void ExecuteNextTutorialElement()
    {
        if (!_tutorialElements.Any())
        {
            _tutorialSequence.OnTutorialFinished();
            return;
        }

        var element = _tutorialElements.Dequeue();

        if (element is TutorialDialogue dialogue)
        {
            if (!_wasLastDialogue) _currentBatchIndex++;
            _wasLastDialogue = true;
            PlayerOnTurn = PlayerCharacter.None;
            ShowTutorialDialogue(dialogue);
            ServiceLocator.Get<IRulesSystem>().DisableForcedAction();
        }
        else if (element is TutorialAction action)
        {
            _wasLastDialogue = false;
            if (action.IsPlayerAction)
            {
                _isCurrentPlayerAction = true;
                PlayerOnTurn = _tutorialSequence.LocalPlayer;
                ServiceLocator.Get<IModel>().AdvanceTurn(PlayerOnTurn);
                ServiceLocator.Get<IRulesSystem>().SetForcedAction(action.ForcedActions, action.ForceOnlyActionItem);
            }
            else
            {
                PlayerOnTurn = PlayerCharacter.None;
                ServiceLocator.Get<IRulesSystem>().DisableForcedAction();
                ServiceLocator.Get<IExecutor>().ExecuteRulesEffects(action.GetEffectCommands());
            }
        }
        OnTurnChanged?.Invoke(PlayerOnTurn);
    }

    private void ShowTutorialDialogue(TutorialDialogue dialogue)
    {
        _tutorialRulebook.DisplayTutorialDialogue(dialogue, EndAction);
    }

    public void RepeatLastTutorialDialogueBatch()
    {
        if (!_isCurrentPlayerAction) return;
        
        var batch = _dialogueBatches[_currentBatchIndex];
        StartCoroutine(RepeatDialogues(batch));
    }

    private IEnumerator RepeatDialogues(TutorialDialogue[] dialogues)
    {
        _isRepeating = true;
        foreach(var dialogue in dialogues)
        {
            bool finished = false;
            _tutorialRulebook.DisplayTutorialDialogue(dialogue, () => finished = true);

            yield return new WaitUntil(() => finished);
            yield return null;
        }

        _isRepeating = false;
    }

    private void InitTutorialElements()
    {
        _tutorialSequence = TutorialProgression.Instance.GetTutorial();
        _tutorialElements = new();
        _dialogueBatches = new();

        var batch = new List<TutorialDialogue>();
        
        foreach (var element in _tutorialSequence.GetTutorialElements())
        {
            _tutorialElements.Enqueue(element);

            if (element is TutorialDialogue tutorialDialogue)
            {
                batch.Add(tutorialDialogue);
            }
            else if (batch.Count > 0)
            {
                _dialogueBatches.Add(batch.ToArray());
                batch.Clear();
            }
        }
    }

    
    
    
    public void ChangeTurn(PlayerCharacter playerOnTurn) {}

    public void SyncRNGs()
    {
        ServiceLocator.Get<IRNG>().Init(new System.Random().Next());
    }
    public void SendTurnChange(PlayerCharacter playerOnTurn) {}
}
