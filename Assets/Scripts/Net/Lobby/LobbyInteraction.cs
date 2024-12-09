using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyInteraction : MonoBehaviour
{
    [SerializeField] private GameObject _hostButton, _clientButton, _codeInput, _mmButton;
    [SerializeField] private TextMeshProUGUI _hostCodeText, _playerCountText;
    private string _clientCode;
    private LobbyManager _lobbyManager;
    private LobbyNetwork _lobbyNetwork;

    private void Awake()
    {
        _lobbyManager = GetComponent<LobbyManager>();
        _lobbyNetwork = GetComponent<LobbyNetwork>();
        _lobbyNetwork.OnPlayerCountChange += UpdatePlayerCount;
    }

    private void OnDisable()
    {
        if (_lobbyNetwork is not null) _lobbyNetwork.OnPlayerCountChange -= UpdatePlayerCount;
    }

    public void HostButton()
    {
        _lobbyManager.StartHostAsync(OnHostStartedLocal);
    }

    public void ClientButton()
    {
        _lobbyManager.StartClientAsync(_clientCode, OnClientStartedLocal);
    }

    public void MatchmakingButton()
    {
        _lobbyManager.CreateorJoinMatchmakingLobby(OnMatchmakingStartedLocal);
    }

    
    public void OnEnterCode(string code)
    {
        _clientCode = code;
    }

    public void UpdatePlayerCount(int count)
    {
        _playerCountText.text = $"players: {count}";
    }

    private void OnHostStartedLocal(string joinCode)
    {
        /*
        _hostButton.SetActive(false);
        _clientButton.SetActive(false);
        _mmButton.SetActive(false);
        _codeInput.SetActive(false);
        */
        _hostCodeText.text = $"{joinCode}";
    }
    
    private void OnMatchmakingStartedLocal(string info)
    {
        /*
        _hostButton.SetActive(false);
        _clientButton.SetActive(false);
        _mmButton.SetActive(false);
        _codeInput.SetActive(false);
        */
        _hostCodeText.text = $"En sala de Matchmaking: {info}";
        Debug.Log("isHost: " + NetworkManager.Singleton.IsHost);
    }

    private void OnClientStartedLocal()
    {
        /*
        _hostButton.SetActive(false);
        _clientButton.SetActive(false);
        _mmButton.SetActive(false);
        _codeInput.SetActive(false);
        */
    }
}
