using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayableToken : APlayableItem, IRulebookEntry
{
    // public override bool OnlyVisibleOnOverview => true;
    public override bool OnlyVisibleOnOverview => false;
    public override bool CanInteractWithoutOwnership => true;
    public override AActionItem ActionItem => _token;

    [SerializeField] private AToken _token;
    [SerializeField] private PlayerCharacter _owner;
    [SerializeField] private Material _transparentMat;

    private Material _defaultMaterial;
    private MeshRenderer[] _renderers;

    public string GetName() => _token.Name;

    public string GetDescription() => _token.Description;


    protected override void Awake()
    {
        base.Awake();
        InHandPosition = transform.position;
        InHandRotation = transform.rotation;
        Owner = _owner;

        _renderers = GetComponentsInChildren<MeshRenderer>();
        _defaultMaterial = _renderers[0].material;
    }

    public void Play(IActionReceiver playLocation, Action onPlayedCallback)
    {
        if (CurrentState is not State.Playable)
        {
            ReturnToHand(onPlayedCallback);
            return;
        }

        if(ActionItem is MacrofungiToken) SoundManager.Instance.PlaySound("Mushroom");
        else SoundManager.Instance.PlaySound("Construction");

        //no se ha jugado visualmente a la mesa
        Travel(playLocation.GetSnapTransform(Owner), _playTravelDuration, State.Played, () =>
        {
            StartCoroutine(WaitAndDo(.5f, () =>
            {
                ReturnToHand(onPlayedCallback);
            }));
        });
    }
    
    public override void OnDrag()
    {
        base.OnDrag();

        SetTransparency(true);
    }

    public override void OnDrop(IActionReceiver dropLocation)
    {
        base.OnDrop(dropLocation);
        transform.rotation = Quaternion.identity;
        SetTransparency(false);
    }

    public override void OnDragCancel()
    {
        base.OnDragCancel();
        SetTransparency(false);
    }

    private IEnumerator WaitAndDo(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }

    private void SetTransparency(bool on)
    {
        foreach (var r in _renderers)
        {
            r.materials = on 
                ? new[]{_transparentMat, _transparentMat} 
                : new[]{_defaultMaterial, _defaultMaterial};
        }
    }
    
}