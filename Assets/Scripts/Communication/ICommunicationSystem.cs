
using System;
using UnityEngine;

public interface ICommunicationSystem : IService
{
    public bool IsAuthority { get; }
    public bool IsRNGSynced { get; }

    public PlayerCharacter LocalPlayer { get; }
    public Camera Camera { get; }

    public event Action<PlayerCharacter, Camera> OnLocalPlayerChange;
    
    public void SyncRNGs();
    
    //manda la accion para ser comprobada y en caso correcto se ejecuta
    public void SendActionToAuthority(PlayerAction action);

    public void SendTurnChange(PlayerCharacter playerOnTurn);

    public void Disconnect();

}
