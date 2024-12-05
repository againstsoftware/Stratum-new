using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Radio : AInteractableObject
{
    [SerializeField] private List<GameObject> radioWheel;
    [SerializeField] private LobbyInteraction _lobbyInteraction;
    public override void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("radio pulsada");
    }

    public void OnButtonCreateLobby()
    {
        // logica pulsado boton de crear lobby
            // girar ruedas
            // mostrar código de sala

        Debug.Log("crear lobby pulsado");

        _lobbyInteraction.HostButton();

        
    }

    public void OnButtonJoinLobby()
    {
        // join lobby
    }
}
