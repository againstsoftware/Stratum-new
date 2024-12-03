
using System;
using UnityEngine;public class AbacusInteraction : MonoBehaviour, IInteractable, IRulebookEntry
{
    public PlayerCharacter Owner => PlayerCharacter.None;

    public bool CanInteractWithoutOwnership => true;
    
    private Vector3 _defaultScale;
    [SerializeField] private float _scaleFactorOnSelected;


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

    public string GetName()
    {
        return LocalizationGod.GetLocalized("Cards", "abacus_name");
    }

    public string GetDescription()
    {
        return LocalizationGod.GetLocalized("Cards", "abacus_desc");
    }

    public event Action OnDiscard;
}
