using System.Collections.Generic;
using Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private LobbyPlayerUISlot _playerUISlotPrefab;  
        [SerializeField] private GameObject _playerSlotsContainer;
        [SerializeField] private ButtonContainer _readyButton;
        [SerializeField] private Button _startMatchButton;  
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _connectButton;
        [SerializeField] private TMP_InputField _userNameInput;
        [SerializeField] private TMP_InputField _addressInput;  
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject _lobbyPanel;   
        private Dictionary<LobbyPlayer, LobbyPlayerUISlot> _playersList = new();
        private LobbyController _lobbyController;
        private LobbyPlayer _localPlayer;    
        public void Initialize(LobbyController lobbyController)
        {
            _lobbyController = lobbyController;
            _hostButton.onClick.AddListener(OnHostButtonClick);
            _connectButton.onClick.AddListener(OnConnectButtonClick);
            _readyButton.Button.onClick.AddListener(OnReadyClick);
            _userNameInput.onEndEdit.AddListener(OnUsernameChanged);
            _startMatchButton.onClick.AddListener(OnStartMatch);
        }   
        private void Awake()
        {
            HideMenu();
            _startMatchButton.gameObject.SetActive(false);
        }   
        public void HideMenu()
        {
            _menuPanel.SetActive(false);
            _lobbyPanel.SetActive(false);
        }   
        public void GotoConnectionForm()
        {
            _lobbyPanel.SetActive(false);
            _menuPanel.SetActive(true);
        }   
        public void GotoLobby(bool isServer)
        {
            _readyButton.enabled = false;
            _lobbyPanel.SetActive(true);
            _menuPanel.SetActive(false);
            _startMatchButton.gameObject.SetActive(isServer);
            ToggleStartMatchBtn(false);
        }   
        public void ToggleStartMatchBtn(bool active) =>
            _startMatchButton.interactable = active; 
    
        public void DisplayPlayers(List<LobbyPlayer> players)
        {
            ClearPlayersList(); 
            foreach (var roomPlayer in players)
                DisplayPlayer(roomPlayer);
        }   
        public void DisplayPlayer(LobbyPlayer player)
        {
            LobbyPlayerUISlot playerScoreUI = SpawnLobbyPlayerSlot(player);
            _playersList.Add(player, playerScoreUI);
        }   
        public void RemovePlayer(LobbyPlayer player)
        {
            Destroy(_playersList[player].gameObject);
            _playersList.Remove(player);
        }

        public void AttachLocalPlayer(LobbyPlayer lobbyPlayer)
        {
            _readyButton.enabled = true;
            _localPlayer = lobbyPlayer;
            _localPlayer.OnReadyChanged += OnLocalPlayerReadyChanged;
        }
    
        private void OnUsernameChanged(string value) =>
            _lobbyController.SetUsername(value);
    
        private void OnConnectButtonClick() =>
            _lobbyController.Connect(_addressInput.text);
    
        private void OnHostButtonClick() =>
            _lobbyController.HostGame();
    
        private void OnStartMatch() =>
            _lobbyController.StartMatch();
    
        private void OnReadyClick() =>
            _lobbyController.TogglePlayerReady();
    
        private void OnLocalPlayerReadyChanged(LobbyPlayer _, bool ready) =>
            _readyButton.Text.text = ready ? "Unready" : "Ready";
    
        private LobbyPlayerUISlot SpawnLobbyPlayerSlot(LobbyPlayer player)
        {
            var playerScoreUI = Instantiate(_playerUISlotPrefab, _playerSlotsContainer.transform);
            playerScoreUI.Initialize(player);
            return playerScoreUI;
        }
    
        private void ClearPlayersList()
        {
            foreach (var pair in _playersList)
                Destroy(pair.Value.gameObject);
        }
    
        private void OnDestroy()
        {
            if (_localPlayer)
                _localPlayer.OnReadyChanged -= OnLocalPlayerReadyChanged;
    
            _hostButton.onClick.RemoveListener(OnHostButtonClick);
            _connectButton.onClick.RemoveListener(OnConnectButtonClick);
            _readyButton.Button.onClick.RemoveListener(OnReadyClick);
            _userNameInput.onEndEdit.RemoveListener(OnUsernameChanged);
        }
    }
}
