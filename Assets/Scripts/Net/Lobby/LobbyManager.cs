using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private int _maxConnections;

    private const string JoinCodeKey = "j";
    private UnityTransport _unityTransport;
    private Lobby _connectedLobby;
    private QueryResponse _lobbies;

    private void Awake()
    {
        _unityTransport = FindObjectOfType<UnityTransport>();
    }

    public void StartHostAsync(Action<string> callback)
    {
        StartCoroutine(StartHostCoroutine(callback));
    }

    public void StartClientAsync(string joinCode, Action callback)
    {
        StartCoroutine(StartClientCoroutine(joinCode, callback));
    }

    public void CreateorJoinMatchmakingLobby(Action<string> callback)
    {
        StartCoroutine(CreateorJoinMatchmakingLobbyCoroutine(callback));
    }

    private IEnumerator StartHostCoroutine(Action<string> callback)
    {
        Task<string> hostTask = StartHostWithRelay();
        while (!hostTask.IsCompleted) yield return null;

        if (hostTask.Status == TaskStatus.RanToCompletion)
        {
            Debug.Log("Host created successfully.");
            callback(hostTask.Result);
        }
        else
        {
            Debug.Log($"Host Task Canceled or Faulted: {hostTask.Exception}");
        }
    }

    private IEnumerator StartClientCoroutine(string joinCode, Action callback)
    {
        Task<bool> clientTask = StartClientWithRelay(joinCode);
        while (!clientTask.IsCompleted) yield return null;

        if (clientTask.Status == TaskStatus.RanToCompletion)
        {
            if (!clientTask.Result) Debug.Log("Client failed.");
            else
            {
                Debug.Log("Client created successfully.");
                callback();
            }
        }
        else
        {
            Debug.Log($"Client Task Canceled or Faulted: {clientTask.Exception}");
        }
    }

    private IEnumerator CreateorJoinMatchmakingLobbyCoroutine(Action<string> callback)
    {
        Task<string> matchmakingTask = CreateOrJoinMatchmakingLobbyAsync();
        while (!matchmakingTask.IsCompleted) yield return null;

        if (matchmakingTask.Status == TaskStatus.RanToCompletion)
        {
            Debug.Log("Matchmaking lobby created or joined successfully.");
            callback(matchmakingTask.Result);
        }
        else
        {
            Debug.LogError($"Matchmaking Task Canceled or Faulted: {matchmakingTask.Exception}");
        }
    }

    private async Task<string> StartHostWithRelay()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(_maxConnections - 1);
        _unityTransport.SetRelayServerData(new RelayServerData(allocation, "wss"));
        _unityTransport.UseWebSockets = true;

        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        _unityTransport.SetRelayServerData(new RelayServerData(joinAllocation, "wss"));
        _unityTransport.UseWebSockets = true;
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }

    private async Task<string> CreateOrJoinMatchmakingLobbyAsync()
    {
        var options = new InitializationOptions();
        options.SetProfile(Random.Range(0, 200000).ToString());
        await UnityServices.InitializeAsync(options);
        AuthenticationService.Instance.SignOut();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        string info = "Client";
        _connectedLobby = await QuickJoinLobby();
        if (_connectedLobby is null)
        {
            info = "Host";
            _connectedLobby = await CreateLobby();
        }

        return info + ", Lobby ID -> " + _connectedLobby.Id;
    }

    private async Task<Lobby> CreateLobby()
    {
        try
        {
            var a = await RelayService.Instance.CreateAllocationAsync(_maxConnections);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                    { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };
            var lobby = await Lobbies.Instance.CreateLobbyAsync("Useless Lobby Name", _maxConnections, options);

            StartCoroutine(WaitUntil(() => lobby != null, () =>
            {
                StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 5f));
                Debug.Log("HeartbeatLobbyCoroutine started for lobby: " + lobby.Id);
            }));

            _unityTransport.SetRelayServerData(new RelayServerData(a, "wss"));
            _unityTransport.UseWebSockets = true;

            NetworkManager.Singleton.StartHost();
            Debug.Log("Matchmaking lobby created!");
            Debug.Log("lobby created: " + lobby);
            return lobby;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed creating a lobby");
            return null;
        }
    }
    private IEnumerator WaitUntil(Func<bool> condition, Action onComplete)
    {
        yield return new WaitUntil(condition);

        onComplete?.Invoke();
    }

    private async Task<Lobby> QuickJoinLobby()
    {
        try
        {
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            _unityTransport.SetRelayServerData(new RelayServerData(a, "wss"));
            _unityTransport.UseWebSockets = true;

            NetworkManager.Singleton.StartClient();
            Debug.Log("Matchmaking lobby joined!");
            return lobby;
        }
        catch (Exception e)
        {
            Debug.LogError("No lobbies available via quick join");
            return null;
        }
    }

    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float intervalSeconds)
    {
        while (_connectedLobby != null)
        {
            Task task = SendHeartbeatAsync(lobbyId);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to send heartbeat: {task.Exception.Message}");
                break;
            }

            yield return new WaitForSeconds(intervalSeconds);
        }
        Debug.Log("connected lobby: " + _connectedLobby);
    }


    private async Task SendHeartbeatAsync(string lobbyId)
    {
        try
        {
            Debug.Log("heartbeat");
            await Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending heartbeat: {e.Message}");
            throw;
        }
    }

}
