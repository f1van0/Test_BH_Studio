using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Game Settings", fileName = "GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        [field: Tooltip("Charging distance"), SerializeField]
        public float ChargeDistance { get; private set; }

        [field: Tooltip("Charge duration"), SerializeField]
        public float ChargingTime { get; private set; }

        [field: Tooltip("Duration of player invincible state"), SerializeField]
        public float InvincibilityDuration { get; private set; }

        [field: Tooltip("Amount of score points"), SerializeField]
        public float RequiredScoreToFinish { get; private set; }

        [field: Tooltip("Delay before next match"), SerializeField]
        public int MatchReloadDelay { get; private set; }

        public GameObject PlayerScorePrefab;
    }
}