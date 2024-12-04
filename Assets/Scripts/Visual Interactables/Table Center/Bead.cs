
using System;
using UnityEngine;

public class Bead : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    
    private static readonly int _appear = Animator.StringToHash("appear");
    private static readonly int _disappear = Animator.StringToHash("disappear");

    private Action _appearCallback;
    private Action _disappearCallback;
    
    public void Appear(Action callback)
    {
        gameObject.SetActive(true);
        _appearCallback = callback;
        _animator.Play(_appear);
    }
    
    public void Disappear(Action callback)
    {
        _disappearCallback = callback;
        _animator.Play(_disappear);
    }

    public void OnAppear()
    {
        _appearCallback?.Invoke();
        _appearCallback = null;
    }
    
    
    public void OnDisappear()
    {
        gameObject.SetActive(false);
        _disappearCallback?.Invoke();
        _disappearCallback = null;
    }
}
