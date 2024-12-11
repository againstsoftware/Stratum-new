using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AInteractableObject : MonoBehaviour, IMenuInteractable
{
    protected float scaleIncrease = 1.2f;
    protected Vector3 _defaultScale;
    protected bool _isEnabled;
    public abstract void OnPointerClick(PointerEventData eventData);
    public abstract void UpdateText();
    [SerializeField] protected InteractionSystemMenu _interactionSystemMenu;


    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if(!_isEnabled) gameObject.transform.localScale = _defaultScale * scaleIncrease;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if(!_isEnabled) gameObject.transform.localScale = _defaultScale;
    }

    public virtual void EnableInteraction()
    {
        _isEnabled = true;
    }

    public virtual void DisableInteraction()
    {
        _isEnabled = false;

        gameObject.transform.localScale = _defaultScale;
    }

    protected virtual void Awake()
    {
        _defaultScale = transform.localScale;
    }

    protected void OnEnable()
    {
        if (_interactionSystemMenu != null)
        {
            Debug.Log("on enable");
            _interactionSystemMenu.OnChangedLanguage += UpdateText;
        }
    }

    protected void OnDisable()
    {
        if (_interactionSystemMenu != null)
        {
            _interactionSystemMenu.OnChangedLanguage -= UpdateText;
        }
    }

}
