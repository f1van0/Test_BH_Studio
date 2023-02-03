using System;
using Mirror;
using UnityEngine;

namespace Script.Lobby
{
    public class LobbyPlayer : NetworkRoomPlayer
    {
        public event Action<LobbyPlayer> OnRoomEnter;
        public event Action<LobbyPlayer> OnRoomExit;
        public event Action<LobbyPlayer, string> OnUsernameChanged;
        public event Action<LobbyPlayer, bool> OnReadyChanged;

        [SyncVar(hook = nameof(UsernameUpdatedCallback))]
        public string Username;

        private void UsernameUpdatedCallback(string oldValue, string newValue) =>
            OnUsernameChanged?.Invoke(this, newValue);

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState) =>
            OnReadyChanged?.Invoke(this, newReadyState);

        [Command]
        public void CmdSetUsername(string value)
        {
            Username = value;
            OnUsernameChanged?.Invoke(this, value);
        }

        public override void OnStartClient()
        {
            Debug.Log(netIdentity);
        }

        public override void OnClientEnterRoom()
        {
            OnRoomEnter?.Invoke(this);
            var roomManager = (AdvanceNetworkManager) NetworkManager.singleton;
            roomManager.OnRoomPlayerConnected(netIdentity, this);
        }

        public override void OnClientExitRoom()
        {
            OnRoomExit?.Invoke(this);
            var roomManager = (AdvanceNetworkManager) NetworkManager.singleton;
            roomManager.OnRoomPlayerDisconnected(netIdentity, this);
        }
    }
}