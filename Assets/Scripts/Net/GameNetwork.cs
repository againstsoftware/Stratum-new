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

    public NetworkVariable<int> RandomSeed { get; private set; } = new(-1);
    public NetworkVariable<bool> IsRandomSeedInit { get; private set; } = new(false);

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
        if (!IsServer)
        {
            // bool init = IsRandomSeedInit.Value;
            // Debug.Log($"spawneado gamenetwork: {IsSpawned}");
            // Debug.Log($"random inicializado en server: {init}");
            if (IsRNGSynced) return;
            
            if (IsRandomSeedInit.Value)
            {
                Debug.Log($"early init seed: {RandomSeed.Value}");
                SetRNGSeed(RandomSeed.Value);
            }
            else
            {
                Debug.Log($"Seed no sincronizada aun, su valor es {RandomSeed.Value}. Peticion al server...");
                // RandomSeed.OnValueChanged += OnRandomSeedInit;
                //le pedimos al server que nos de la key
                QuerySeedToServerRpc(LocalPlayer);
            }

            return;
        }
        Debug.Log("generando la seed en el server...");
        var random = new System.Random();
        RandomSeed.Value = random.Next();
        IsRandomSeedInit.Value = true;
        SendSeedToClientRpc(RandomSeed.Value);
    }

    // private void OnRandomSeedInit(int __, int _)
    // {
    //     RandomSeed.OnValueChanged -= OnRandomSeedInit;
    //     
    //     Debug.Log($"late init seed: {RandomSeed.Value}");
    //     SetRNGSeed(RandomSeed.Value);
    // }

    [ServerRpc(RequireOwnership = false)]
    private void QuerySeedToServerRpc(PlayerCharacter clientCharacter)
    {
        Debug.Log("recibida peticion de seed");
        if(IsRandomSeedInit.Value) 
            SendSeedToSpecificClientRpc(clientCharacter, RandomSeed.Value);
        else
            Debug.Log("random aun no iniciado, cuando se inicie se mandara la seed a todos.");
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


    [ClientRpc]
    private void SendSeedToClientRpc(int seed)
    {
        Debug.Log("Recibiendo la seed en el cliente...");
        SetRNGSeed(seed);
    }
    
    [ClientRpc]
    private void SendSeedToSpecificClientRpc(PlayerCharacter clientCharacter, int seed)
    {
        if (LocalPlayer != clientCharacter) return;
        Debug.Log("Recibiendo la seed en el cliente especifico...");
        SetRNGSeed(seed);
    }

    private void SetRNGSeed(int seed)
    {
        ServiceLocator.Get<IRNG>().Init(seed);
        IsRNGSynced = true;
        Debug.Log($"random sync en el cliente. seed: {seed}");
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