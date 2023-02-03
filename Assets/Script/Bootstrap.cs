using System.Collections;
using System.Collections.Generic;
using Script.Lobby;
using Script.Services;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    public AdvanceNetworkManager AdvanceNetworkManagerPrefab;
    public LobbyUI LobbyUIPrefab;
    
    void Start()
    {
        var roomManager = Instantiate(AdvanceNetworkManagerPrefab);
        var lobbyUI = Instantiate(LobbyUIPrefab);
        var lobbyController = new LobbyController(roomManager, lobbyUI);
        roomManager.Initialize(lobbyController);
        var authenticator = roomManager.authenticator as ClientAuthenticator;
        lobbyUI.Initialize(lobbyController);
        DontDestroyOnLoad(lobbyUI);
    
        lobbyController.GotoConnection();
    }
}
