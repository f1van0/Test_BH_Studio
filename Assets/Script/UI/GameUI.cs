using System.Collections;
using System.Collections.Generic;
using Script.Data;
using Script.Game.Character;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.UI
{
    public class GameUI : MonoBehaviour
    {
        [field: SerializeField] private Canvas _endGameCanvas;
        [field: SerializeField] private TMP_Text _winnerText;
        [field: SerializeField] private TMP_Text _timerText;
        private GameSettings _gameSettings;
        private GameLoopManager _gameLoopManager;

        private Dictionary<uint, PlayerScoreUI> _scores = new();

        public void Initialize(GameSettings gameSettings, GameLoopManager gameLoopManager)
        {
            _gameSettings = gameSettings;
            _gameLoopManager = gameLoopManager;
            _gameLoopManager.HitRegistered += UpdatePlayerScore;
        }

        public void ShowWinner(CharacterContainer player, int score, int countDownSeconds)
        {
            _endGameCanvas.enabled = true;
            _winnerText.text = $"Winner: {player.lobbyPlayer.Username}, Score: {score}";
            StartCoroutine(ShowCountDown(countDownSeconds));
        }

        private IEnumerator ShowCountDown(int countDownSeconds)
        {
            int left = countDownSeconds;
            while (left >= 0)
            {
                _timerText.text = left.ToString();
                yield return new WaitForSecondsRealtime(1);
                left--;
            }
        }

        public void DisplayPlayerScore(CharacterContainer container)
        {
            var scoreGO = Object.Instantiate(_gameSettings.PlayerScorePrefab, container.ScorePosition);
            var scoreUI = scoreGO.GetComponent<PlayerScoreUI>();
            scoreGO.transform.rotation = Quaternion.Euler(0, 180, 0);
            scoreUI.SetUsername(container.lobbyPlayer.Username);
            scoreUI.SetScore(0);

            Debug.Log($"UI attached to {container.netId}");
            _scores.Add(container.netId, scoreUI);
        }

        private void UpdatePlayerScore(CharacterContainer container, int score) =>
            _scores[container.netId].SetScore(score);

        public void Dispose()
        {
            _gameLoopManager.HitRegistered -= UpdatePlayerScore;
        }
    }
}