
using System.Collections.Generic;
using UnityEngine;

public class Disconnection : ATutorialSequence
{
    public override PlayerCharacter LocalPlayer { get; protected set; } = PlayerCharacter.Sagitario;
    [SerializeField] private TutorialDialogue[] _dialogues;
    [SerializeField] private string _returnScene;
    public override IEnumerable<ITutorialElement> GetTutorialElements()
    {
        return _dialogues;
    }

    public override void OnTutorialFinished()
    {
        SceneTransition.Instance.TransitionToScene(_returnScene);
    }
}
