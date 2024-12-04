using System;
using System.Collections;
using UnityEngine;public class TutorialRulebookInteraction : MonoBehaviour, IInteractable, IRulebookEntry
{
    public PlayerCharacter Owner => PlayerCharacter.None;

    public bool CanInteractWithoutOwnership => true;
    
    
    [SerializeField] private float _scaleFactorOnSelected = 1.1f;
    [SerializeField] private string _nameKey;
    [SerializeField] private string _descriptionKey;

    private Vector3 _defaultScale;
    private TutorialManager _tutorialManager;
    private bool _canInteract = false;
    private void Awake()
    {
        _defaultScale = transform.localScale;
        _tutorialManager = FindAnyObjectByType<TutorialManager>();
    }

    private IEnumerator Start() //esto es guarrada de manual pero es la que hay, es para que no se clicke muy al principio
    {
        yield return new WaitForSeconds(3f);
        _canInteract = true;
    }


    public void OnSelect()
    {
        if(_tutorialManager.CanTriggerRepeat && _canInteract)
            transform.localScale = _defaultScale * _scaleFactorOnSelected;
    }

    public void OnDeselect()
    {
        transform.localScale = _defaultScale;
    }

    public void OnPress()
    {
        if(_tutorialManager.CanTriggerRepeat && _canInteract)
            _tutorialManager.RepeatLastTutorialDialogueBatch();
    }
    public void OnRelease() {}

    public string GetName()
    {
        return LocalizationGod.GetLocalized("Cards", _nameKey);
    }

    public string GetDescription()
    {
        return LocalizationGod.GetLocalized("Cards", _descriptionKey);
    }

    public event Action OnDiscard;
}