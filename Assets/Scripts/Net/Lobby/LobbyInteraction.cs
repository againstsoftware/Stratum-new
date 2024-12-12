using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class LobbyInteraction : MonoBehaviour
{
    //[SerializeField] private GameObject _hostButton, _clientButton, _codeInput, _mmButton;
    [SerializeField] public TextMeshProUGUI  _playerCountText;
    [SerializeField] private TMP_InputField _hostCodeText;
    [SerializeField] private TextMeshProUGUI _stateInfoText;
    private string _clientCode;
    private LobbyManager _lobbyManager;
    private LobbyNetwork _lobbyNetwork;
    public string lastTableKey;

    private void Awake()
    {
        _lobbyManager = GetComponent<LobbyManager>();
        _lobbyNetwork = GetComponent<LobbyNetwork>();
        _lobbyNetwork.OnPlayerCountChange += UpdatePlayerCount;
        lastTableKey = null;
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
        if(NetworkManager.Singleton.IsHost) return;
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
        _playerCountText.text = $"{count} / 4";
    }

    private void OnHostStartedLocal(string joinCode)
    {
        /*
        _hostButton.SetActive(false);
        _clientButton.SetActive(false);
        _mmButton.SetActive(false);
        _codeInput.SetActive(false);
        */
        UpdateStateText("createdlobby_state");
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
        //_hostCodeText.text = $"En sala de Matchmaking: {info}";

        // debug
        //Debug.Log("isHost: " + NetworkManager.Singleton.IsHost);
    }

    private void OnClientStartedLocal()
    {
        /*
        _hostButton.SetActive(false);
        _clientButton.SetActive(false);
        _mmButton.SetActive(false);
        _codeInput.SetActive(false);
        */
        UpdateStateText("joinedlobby_state");
    }

    public void UpdateStateText(string tableKey)
    {
        lastTableKey = tableKey;
        _stateInfoText.text = LocalizationGod.GetLocalized("RadioInfo", tableKey);
    }
}
