using System.Collections.Generic;
using System.Net;
using Mirror;
using Script.Data;
using UnityEngine;

namespace Script.Lobby
{
    public class LobbyController
    {
        private readonly AdvanceNetworkManager _advanceNetworkManager;
        private readonly LobbyUI _lobbyUI;
        private LobbyPlayer _localPlayer;
        public List<NetworkRoomPlayer> Players => _advanceNetworkManager.PlayerSlots;

        public LobbyController(AdvanceNetworkManager advanceNetworkManager, LobbyUI lobbyUI)
        {
            _lobbyUI = lobbyUI;
            _advanceNetworkManager = advanceNetworkManager;
            _advanceNetworkManager.ClientEnterRoom += OnClientConnected;
            _advanceNetworkManager.ClientExitRoom += OnClientDisconnected;
            _advanceNetworkManager.PlayersReady += OnPlayersReady;
        }

        public void GotoConnection() =>
            _lobbyUI.GotoConnectionForm();

        public void HostGame()
        {
            _advanceNetworkManager.StartHost();
            _lobbyUI.GotoLobby(true);
        }

        public void Connect(string host)
        {
            if (LocalPlayerInfo.Username == "") return;

            IPAddress ipAddress;
            if (host == "localhost" || host == "127.0.0.1")
                ipAddress = IPAddress.Loopback;
            else
                ipAddress = IPAddress.Parse(host);

            _advanceNetworkManager.Connect(ipAddress);
            _lobbyUI.GotoLobby(false);
        }

        public void StartMatch() =>
            _advanceNetworkManager.StartMatch();

        public void TogglePlayerReady() =>
            _localPlayer.CmdChangeReadyState(!_localPlayer.readyToBegin);

        public void SetUsername(string username) =>
            LocalPlayerInfo.Username = username;

        private void OnClientConnected(NetworkIdentity identity, LobbyPlayer player)
        {
            _lobbyUI.DisplayPlayer(player);

            if (identity.isLocalPlayer)
            {
                _localPlayer = player;
                _lobbyUI.AttachLocalPlayer(_localPlayer);
            }
        }

        private void OnClientDisconnected(NetworkIdentity identity, LobbyPlayer player) =>
            _lobbyUI.RemovePlayer(player);

        private void OnPlayersReady(bool value)
        {
            if (value)
                Debug.Log("All players are ready");
            _lobbyUI.ToggleStartMatchBtn(value);
        }

        public void Hide()
        {
            _lobbyUI.HideMenu();
        }
    }
}