using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections;

public class LobbyNetwork : NetworkBehaviour
{
    public event System.Action<int> OnPlayerCountChange;
    private NetworkVariable<int> _playerCount = new(0);

    //server only
    private Dictionary<PlayerCharacter, NetworkPlayer> _networkPlayers = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Host updates player count when players connect/disconnect
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        if (IsClient)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCheckIfHost;
        }

        _playerCount.OnValueChanged += OnPlayerCountChanged;

        base.OnNetworkSpawn();

        //BORRAR
        StartCoroutine(ExecuteEverySecond());
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        _playerCount.OnValueChanged -= OnPlayerCountChanged;

        base.OnNetworkDespawn();
    }

    private void OnClientDisconnectedCheckIfHost(ulong clientID)
    {
        Debug.Log("OnclientdisconnectedCheckIfHost");
        if (IsClient)
        {
            if (clientID == NetworkManager.ServerClientId)
            {
                HandleHostDisconnection();
            }
        }
    }

    private void HandleHostDisconnection()
    {
        Debug.Log("host desconectado");
        StartCoroutine(ShutdownAndWaitCoroutine());
        Debug.Log("networkmanager " + NetworkManager.Singleton);
    }

    private IEnumerator ShutdownAndWaitCoroutine()
    {
        Debug.Log("Iniciando el apagado de NetworkManager...");

        // Llamar al apagado
        NetworkManager.Singleton.Shutdown();

        // Esperar hasta que el NetworkManager deje de estar activo
        yield return new WaitUntil(() => !NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient);

        Debug.Log("NetworkManager se ha apagado completamente.");
        // Aquí puedes continuar con otras acciones después del apagado
    }

    private IEnumerator ExecuteEverySecond()
    {
        bool isRunning = true;
        while (isRunning)
        {
            if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("es cliente ");
            }
            else
            {
                Debug.Log("No hay clientes conectados al servidor.");
            }

            if(NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("es cliente conectado");
            }

            yield return new WaitForSeconds(1f);
        }
    }



    private void OnClientConnected(ulong clientID)
    {
        if (IsServer) _playerCount.Value++;
    }

    private void OnClientDisconnected(ulong clientID)
    {
        if (IsServer) _playerCount.Value--;
    }

    private void OnPlayerCountChanged(int lastV, int newV)
    {
        OnPlayerCountChange?.Invoke(newV);

        if (newV != 4 || !IsServer) return;

        var players =
            new List<NetworkPlayer>(FindObjectsByType<NetworkPlayer>(FindObjectsInactive.Exclude,
                FindObjectsSortMode.None));
        var characters = new List<PlayerCharacter>(new[]
            { PlayerCharacter.Ygdra, PlayerCharacter.Sagitario, PlayerCharacter.Fungaloth, PlayerCharacter.Overlord });

        while (players.Any())
        {
            int randomIndex = Random.Range(0, players.Count);
            var player = players[randomIndex];
            players.RemoveAt(randomIndex);
            var character = characters[^1];
            characters.RemoveAt(characters.Count - 1);
            _networkPlayers.Add(character, player);
            player.SetCharacter(character);
        }


        // NetworkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        SceneTransition.Instance.TransitionToScene("Game", true);


    }




}
