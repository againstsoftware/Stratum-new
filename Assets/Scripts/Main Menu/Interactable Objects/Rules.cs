using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Rules : AInteractableObject
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI _leftPage;
    private Animator _animator;
    private float waitTime = 0.3f;

    protected override void  Awake()
    {
        base.Awake();
        
        _animator = GetComponent<Animator>();
       StartCoroutine(WaitForInitialization());
    }

    private void Start()
    {
        cameraOffset = new Vector3(0f, 0.8f, -1.5f);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
    }

    public override void EnableInteraction()
    {
        base.EnableInteraction();
        _animator.SetBool("_isEnabled", _isEnabled);
        // if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        // {
        //     StartCoroutine(WaitForOneSecond());
        // }

    }

    public override void DisableInteraction()
    {
        base.DisableInteraction();
        _animator.SetBool("_isEnabled", _isEnabled);
        // if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        // {
        //     HideText();
        // }
    }
    
    private void ShowText()
    {
        foreach (Transform child in _canvas.transform)
        {
            child.gameObject.SetActive(true);
        }
        _leftPage.text = LocalizationGod.GetLocalized("MenuBooks", "rules_left");

    }

    private void HideText()
    {
        foreach (Transform child in _canvas.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    IEnumerator WaitForOneSecond()
    {
        yield return new WaitForSeconds(waitTime);
        if(_isEnabled)
        {
            ShowText();
        }
    }

    public override void UpdateText()
    {
        _leftPage.text = LocalizationGod.GetLocalized("MenuBooks", "rules_left");
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
