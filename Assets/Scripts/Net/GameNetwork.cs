using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;

public class GameNetwork : NetworkBehaviour, ICommunicationSystem
{
    public bool IsAuthority
    {
        get
        {
            if (!IsSpawned)
                throw new Exception("Error! Comprobando autoridad sin estar spawneado");
            return IsServer;
        }
    }

    public bool IsRNGSynced { get; private set; }

    public PlayerCharacter LocalPlayer { get; private set; } = PlayerCharacter.None;
    public Camera Camera { get; private set; }

    public event Action<PlayerCharacter, Camera> OnLocalPlayerChange;


    [SerializeField] private GameConfig _config;


    private void Awake()
    {
        var localID = NetworkManager.Singleton.LocalClientId;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        var networkPlayers = FindObjectsByType<NetworkPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        var localNetworkPlayer = networkPlayers.FirstOrDefault(np => np.ID.Value == localID);
        LocalPlayer = localNetworkPlayer.Character.Value;
        foreach (PlayerCharacter character in Enum.GetValues(typeof(PlayerCharacter)))
        {
            if (character is PlayerCharacter.None) continue;

            var viewPlayer = ServiceLocator.Get<IView>().GetViewPlayer(character);
            var cam = viewPlayer.MainCamera;

            if (character != LocalPlayer)
            {
                Destroy(cam.gameObject);
                Destroy(viewPlayer.UICamera.gameObject);
            }
            else
            {
                //temporal mientras no hay mas luces
                // var lightTransform = FindAnyObjectByType<Light>().transform.parent;
                // lightTransform.LookAt(cam.transform.forward);

                /*var input = */
                FindAnyObjectByType<PlayerInput>().camera = cam;

                viewPlayer.IsLocalPlayer = true;

                Camera = cam;
                OnLocalPlayerChange?.Invoke(LocalPlayer, cam);
            }
        }
    }


    public void SyncRNGs()
    {
        if (!IsServer) return;
        GenerateSeedServerRpc();
    }

    private void OnClientDisconnected(ulong id)
    {
        if (IsServer)
            DisconnectClientRpc();
        else Disconnect();
    }

    [ClientRpc]
    private void DisconnectClientRpc()
    {
       Disconnect();
    }

    private void Disconnect()
    {
        NetworkManager.Singleton.Shutdown();
        SceneTransition.Instance.TransitionToScene("Disconnection");   
    }


    [ServerRpc]
    private void GenerateSeedServerRpc()
    {
        var random = new System.Random();
        var seed = random.Next();
        SendSeedToClientRpc(seed);
    }

    [ClientRpc]
    private void SendSeedToClientRpc(int seed)
    {
        ServiceLocator.Get<IRNG>().Init(seed);
        IsRNGSynced = true;
    }


    public void SendActionToAuthority(PlayerAction action)
    {
        if (!IsSpawned)
            throw new Exception("Error! Mandando rpcs sin estar spawneado");

        if (action.Actor != LocalPlayer)
            throw new Exception("Error! Mandando rpcs de accion sin ser el jugador local");

        ServiceLocator.Get<IExecutor>().ExecutePlayerActionEffects(action);
        if (IsServer) //si somos servidor no hace falta recheck, solo mandamos la accion a los clientes
        {
            SendActionToExecuteInClientRpc(new NetworkPlayerAction(action, _config));
        }
        else // pero si no somos server, enviamos la accion al server para check de trampas
        {
            SendActionToServerRpc(new NetworkPlayerAction(action, _config));
        }
    }

    public void SendTurnChange(PlayerCharacter playerOnTurn)
    {
        SendTurnChangeToClientRpc(playerOnTurn);
    }

    [ClientRpc]
    private void SendTurnChangeToClientRpc(PlayerCharacter playerOnTurn)
    {
        ServiceLocator.Get<ITurnSystem>().ChangeTurn(playerOnTurn);
    }


    [ServerRpc(RequireOwnership = false)]
    private void SendActionToServerRpc(NetworkPlayerAction networkAction) //comporueba las reglas en el server 
    {
        var action = networkAction.ToPlayerAction(_config);

        if (!ServiceLocator.Get<IRulesSystem>().IsValidAction(action))
            throw new Exception($"JUGADOR {action.Actor} HA HECHO TRAMPA! HAY INCONSISTENCIAS!");

        else SendActionToExecuteInClientRpc(networkAction);
    }

    [ClientRpc]
    private void SendActionToExecuteInClientRpc(NetworkPlayerAction action)
    {
        if (action.Actor == LocalPlayer) return;

        ServiceLocator.Get<IExecutor>().ExecutePlayerActionEffects(action.ToPlayerAction(_config));
    }
}