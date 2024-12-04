
using System;
using UnityEngine;public class TableItemInteraction : MonoBehaviour, IInteractable, IRulebookEntry
{
    public PlayerCharacter Owner => PlayerCharacter.None;

    public bool CanInteractWithoutOwnership => true;

    public event Action OnItemPress;
    public event Action OnItemRelease;
    
    private Vector3 _defaultScale;
    [SerializeField] private float _scaleFactorOnSelected = 1.1f;
    [SerializeField] private string _nameKey;
    [SerializeField] private string _descriptionKey;

    private void Awake()
    {
        _defaultScale = transform.localScale;
    }
    
    
    public void OnSelect()
    {
        transform.localScale = _defaultScale * _scaleFactorOnSelected;
    }

    public void OnDeselect()
    {
        transform.localScale = _defaultScale;
    }

    public void OnPress() => OnItemPress?.Invoke();
    public void OnRelease() => OnItemRelease?.Invoke();

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
