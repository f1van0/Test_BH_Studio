using Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LobbyPlayerUISlot : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _readiness;
        private LobbyPlayer _player;

        public void Initialize(LobbyPlayer player)
        {
            _player = player;
            _player.OnReadyChanged += OnPlayerReadyChanged;
            _player.OnUsernameChanged += OnPlayerUsernameChanged;
            OnPlayerUsernameChanged(_player, _player.Username);
            OnPlayerReadyChanged(_player, _player.readyToBegin);
        }

        private void OnPlayerUsernameChanged(LobbyPlayer _, string newName) =>
            _text.text = newName;

        private void OnPlayerReadyChanged(LobbyPlayer _, bool ready) =>
            _readiness.color = ready ? Color.green : Color.red;


        private void OnDestroy()
        {
            _player.OnReadyChanged -= OnPlayerReadyChanged;
            _player.OnUsernameChanged -= OnPlayerUsernameChanged;
        }
    }
}