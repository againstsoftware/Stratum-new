    using System;
    using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Note : AInteractableObject
{
    [SerializeField] private string _key;
    [SerializeField] private TextMeshProUGUI _text;


    private IEnumerator Start()
    {
        yield return new WaitUntil(() => LocalizationGod.IsInitialized);
        UpdateText();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        UpdateText();
    }

    public override void UpdateText()
    {
        _text.text = LocalizationGod.GetLocalized("MenuBooks", _key);
    }
}
