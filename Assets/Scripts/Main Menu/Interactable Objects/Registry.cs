using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Registry : AInteractableObject
{
    private Animator _animator;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TMP_Text _leftText, _rightText;
    [SerializeField] private Collider _Link;

    private float waitTime = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        
        _animator = GetComponent<Animator>();
        //ShowText();
        StartCoroutine(WaitForInitialization());
        _Link.enabled = false;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if(_isEnabled && (eventData.pointerCurrentRaycast.gameObject.GetComponent<Collider>() == _Link))
        {
            Application.OpenURL("https://linktr.ee/againstsoftware");
        }
    }

    public override void EnableInteraction()
    {
        base.EnableInteraction();
        
        _animator.SetBool("_isEnabled", _isEnabled);
        if(_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            StartCoroutine(WaitForOneSecond());
            
        }
    }

    public override void DisableInteraction()
    {
        base.DisableInteraction();
        
        
        _isEnabled = false;
        _animator.SetBool("_isEnabled", _isEnabled);
        // if(_animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        // {
        //     HideText();
        // }
        _Link.enabled = false;
    }

    private void ShowText()
    {
        foreach (Transform child in _canvas.transform)
        {
            child.gameObject.SetActive(true);   
        }
        foreach (Transform child in _canvas.transform)
        {
            child.gameObject.SetActive(true);
        }
        _leftText.text = LocalizationGod.GetLocalized("MenuBooks", "registry_left");
        _rightText.text = LocalizationGod.GetLocalized("MenuBooks", "registry_right");
        /*
        string language = PlayerPrefs.GetString(GamePrefs.LanguagePrefKey, "en");
        if(language == "en")
        {
            _leftText.text = "CREDITS";
            _rightText.text = "Press to visit our social media!";
        }
        if(language == "es")
        {
            _leftText.text = "CRÉDITOS";
            _rightText.text = "¡Pulsa para visitar nuestras redes sociales!";
        }
        */
        
    }

    private void HideText()
    {
        foreach (Transform child in _canvas.transform)
        {
            child.gameObject.SetActive(false);   
        }

        _rightText.text = "";
    }

    IEnumerator WaitForOneSecond()
    {
        yield return new WaitForSeconds(waitTime);
        if(_isEnabled)
        {
            // ShowText();
            _Link.enabled = true;
        }
        
    }

    public override void UpdateText()
    {
       _leftText.text = LocalizationGod.GetLocalized("MenuBooks", "registry_left");
       _rightText.text = LocalizationGod.GetLocalized("MenuBooks", "registry_right");
    }

     private IEnumerator WaitForInitialization()
    {
        while (!LocalizationGod.IsInitialized)
        {
            yield return null;
        }
        ShowText();
    }  
}
