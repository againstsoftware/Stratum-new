
using System;
using UnityEngine;

public class PlayerOrientedCenter : MonoBehaviour
{
    public event Action OnRotateToPlayer;
    private void Start()
    {
        var comms = ServiceLocator.Get<ICommunicationSystem>();
        comms.OnLocalPlayerChange += SetLocalPlayer;
        SetLocalPlayer(comms.LocalPlayer, comms.Camera);
    }
    
    private void SetLocalPlayer(PlayerCharacter localPlayer, Camera cam)
    {
        if (localPlayer is PlayerCharacter.None) return;

        var camPos = cam.transform.position;
        camPos.y = transform.position.y;
        transform.LookAt(camPos);
        OnRotateToPlayer?.Invoke();
    }
}
