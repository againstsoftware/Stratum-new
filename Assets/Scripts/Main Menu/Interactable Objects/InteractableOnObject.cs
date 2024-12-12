using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableOnObject : MonoBehaviour, IInteractableOnObject
{
    public bool isEnabled = false;
    private Vector3 _defaultScale;
    private float scaleIncrease = 1.1f;

    private void Awake()
    {
        _defaultScale = transform.localScale;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isEnabled) gameObject.transform.localScale = _defaultScale * scaleIncrease;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isEnabled) gameObject.transform.localScale = _defaultScale;
    }
}
