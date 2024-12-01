using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = System.Random;

public class TestModeCommunications : MonoBehaviour, ICommunicationSystem
{
    public bool IsAuthority => true;
    public bool IsRNGSynced => true;

    public PlayerCharacter LocalPlayer { get; private set; } = PlayerCharacter.None;
    public Camera Camera { get; private set; }


    public event Action<PlayerCharacter, Camera> OnLocalPlayerChange;

    
    [SerializeField] private GameConfig _config;

    private List<ViewPlayer> _players = new();

    private ViewPlayer _playerOnTurn;


    private void Awake()
    {
        foreach (PlayerCharacter character in _config.TurnOrder)
        {
            if (character is PlayerCharacter.None) continue;


            var viewPlayer = ServiceLocator.Get<IView>().GetViewPlayer(character);

            _players.Add(viewPlayer);
        }

        ChangeActivePlayer(_config.TurnOrder[0]);
    }

    public void SyncRNGs()
    {
        ServiceLocator.Get<IRNG>().Init(new Random().Next());
    }

    public void SendActionToAuthority(PlayerAction action)
    {
        ServiceLocator.Get<IExecutor>().ExecutePlayerActionEffects(action);
    }

    public void SendTurnChange(PlayerCharacter playerOnTurn)
    {
        if (playerOnTurn is PlayerCharacter.None)
        {
            _playerOnTurn = null;
        }
        else
        {
            ChangeActivePlayer(playerOnTurn);
        }

        ServiceLocator.Get<ITurnSystem>().ChangeTurn(playerOnTurn);
    }


    private void ChangeActivePlayer(PlayerCharacter newLocalPlayer)
    {
        foreach (var viewPlayer in _players)
        {
            if (viewPlayer.Character is PlayerCharacter.None) continue;

            var cam = viewPlayer.MainCamera;
            var uicam = viewPlayer.UICamera;


            if (viewPlayer.Character != newLocalPlayer)
            {
                cam.enabled = false;
                cam.GetComponent<AudioListener>().enabled = false;
                cam.GetComponent<PhysicsRaycaster>().enabled = false;

                uicam.enabled = false;

                viewPlayer.IsLocalPlayer = false;
            }
            else
            {
                _playerOnTurn = viewPlayer;
                FindAnyObjectByType<PlayerInput>().camera = cam;
                cam.enabled = true;
                cam.GetComponent<AudioListener>().enabled = true;
                cam.GetComponent<PhysicsRaycaster>().enabled = true;
                uicam.enabled = true;
                _playerOnTurn.IsLocalPlayer = true;

                LocalPlayer = _playerOnTurn.Character;
                Camera = cam;
                OnLocalPlayerChange?.Invoke(_playerOnTurn.Character, cam);
                //
                // ServiceLocator.Get<IInteractionSystem>().SetLocalPlayer(_playerOnTurn.Character, cam);
                // ServiceLocator.Get<IView>().SetLocalPlayer(_playerOnTurn.Character, cam);
            }
        }
    }
}