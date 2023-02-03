using TMPro;
using UnityEngine;

namespace Script.UI
{
    public class PlayerScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        private string _username;
        private int _score;

        public void SetUsername(string username)
        {
            _username = username;
            UpdateUI();
        }

        public void SetScore(int score)
        {
            _score = score;
            UpdateUI();
        }

        private void UpdateUI()
        {
            _text.text = $"{_username}: {_score}";
        }
    }
}