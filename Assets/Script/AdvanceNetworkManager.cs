using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using kcp2k;
using Mirror;
using Script.Data;
using Script.Game.Character;
using Script.Lobby;
using Unity.VisualScripting;
using UnityEngine;

public class AdvanceNetworkManager : NetworkRoomManager
{
    public event Action<NetworkConnection, AuthenticationData> ClientConnected;
        public event Action<NetworkConnection, AuthenticationData> ClientDisconnected;
        public event Action<NetworkIdentity, LobbyPlayer> ClientEnterRoom;
        public event Action<NetworkIdentity, LobbyPlayer> ClientExitRoom;

        public GameLoopManager GameLoopManagerPrefab;
        public GameSettings GameSettings;

        private CharacterFactory _characterFactory;
        private LevelStaticData _levelData;
        private GameLoopManager _gameLoopManager;
        private LobbyController _lobbyController;

        public event Action<bool> PlayersReady;

        public Dictionary<NetworkConnection, AuthenticationData> Clients { get; } = new();
        public Dictionary<NetworkIdentity, LobbyPlayer> LocalRoomPlayers { get; } = new();
        private Dictionary<NetworkConnection, LobbyPlayer> _lobbyPlayers;

        public void Initialize(LobbyController lobbyController)
        {
            _lobbyController = lobbyController;
        }

        public void Connect(IPAddress address)
        {
            var uri = BuildURI(address);
            StartClient(uri);
        }

        public void OnRoomPlayerConnected(NetworkIdentity netIdentity, LobbyPlayer player)
        {
            if (LocalRoomPlayers.ContainsKey(netIdentity)) return;
            LocalRoomPlayers.Add(netIdentity, player);
            ClientEnterRoom?.Invoke(netIdentity, player);
        }

        public void OnRoomPlayerDisconnected(NetworkIdentity netIdentity, LobbyPlayer player)
        {
            if (!LocalRoomPlayers.ContainsKey(netIdentity)) return;
            LocalRoomPlayers.Remove(netIdentity);
            ClientExitRoom?.Invoke(netIdentity, player);
        }

        public override void OnRoomServerConnect(NetworkConnectionToClient conn)
        {
            var data = conn.authenticationData as AuthenticationData;
            Clients.Add(conn, data);
            ClientConnected?.Invoke(conn, data);
        }

        public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
        {
            if (Clients.TryGetValue(conn, out var data))
                ClientDisconnected?.Invoke(conn, data);
        }

        public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
        {
            LobbyPlayer lobbyPlayer = Instantiate(roomPlayerPrefab, Vector3.zero, Quaternion.identity) as LobbyPlayer;
            if (lobbyPlayer == null)
            {
                Debug.LogError("Room prefab missing RoomPlayer script");
                return null;
            }

            var authData = conn.authenticationData as AuthenticationData;
            lobbyPlayer.Username = authData.Username;
            return lobbyPlayer.gameObject;
        }

        public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject lobbyPlayer)
        {
            var roomPlayer = lobbyPlayer.GetComponent<LobbyPlayer>();
            var container = SpawnGamePlayer(roomPlayer);
            return container.gameObject;
        }

        public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn)
        {
            var roomPlayer = _lobbyPlayers[conn];
            var container = SpawnGamePlayer(roomPlayer);
            NetworkServer.AddPlayerForConnection(conn, container.gameObject);
        }

        private CharacterContainer SpawnGamePlayer(LobbyPlayer lobbyPlayer)
        {
            var container = _characterFactory.SpawnCharacter(lobbyPlayer);
            container.gameObject.name += $" [netId = {lobbyPlayer.netId}]";
            Debug.Log($"Spawn player {container.netId} for {lobbyPlayer.netId}");
            return container;
        }

        public override void OnRoomServerSceneChanged(string sceneName)
        {
            if (sceneName == GameplayScene)
            {
                _gameLoopManager = Instantiate(GameLoopManagerPrefab);
                NetworkServer.Spawn(_gameLoopManager.gameObject);

                _lobbyController.Hide();
                _levelData = FindObjectOfType<LevelStaticData>();
                _characterFactory = new CharacterFactory(_levelData, playerPrefab);
                _gameLoopManager.Initialize(this, GameSettings, _levelData);
            }
        }

        public void SetupCharacter(CharacterContainer container)
        {
            _gameLoopManager?.SetupPlayer(container);
        }

        public void OnClientArenaLoaded(GameLoopManager gameLoopManager)
        {
            if (NetworkClient.localPlayer.isServer) return;

            _gameLoopManager = gameLoopManager;
            _lobbyController.Hide();
            _levelData = FindObjectOfType<LevelStaticData>();
            _gameLoopManager.Initialize(this, GameSettings, _levelData);
        }

        public void StartMatch()
        {
            FillLobbyPlayers();
            SwitchToArena();
        }

        public void SwitchToArena() =>
            ServerChangeScene(GameplayScene);

        public override void OnRoomServerPlayersReady() =>
            PlayersReady?.Invoke(true);

        public override void OnRoomServerPlayersNotReady() =>
            PlayersReady?.Invoke(false);

        private void FillLobbyPlayers()
        {
            _lobbyPlayers = new();
            foreach (var pair in LocalRoomPlayers)
                _lobbyPlayers.Add(pair.Key.connectionToClient, pair.Value);
        }

        private Uri BuildURI(IPAddress address)
        {
            if (transport is KcpTransport kcpTransport)
            {
                UriBuilder builder = new UriBuilder();
                var exampleURI = kcpTransport.ServerUri();
                builder.Scheme = exampleURI.Scheme;
                builder.Port = exampleURI.Port;
                builder.Host = address.ToString();
                return builder.Uri;
            }

            throw new ArgumentException("Not supported transport");
        }
}
