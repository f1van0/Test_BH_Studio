using System;
using System.Collections;
using Data;
using Mirror;
using UnityEngine;

namespace Game.Character.Ability
{
    public class ChargeAbility : NetworkBehaviour
    {
        [field: SyncVar] public bool IsCharging { get; private set; }
        public event Action<ChargeAbility> Completed;
        public Vector3 ChargingDirection { get; private set; }

        [SerializeField] private CollisionDetector _detector;
        [SerializeField] private CharacterMovement _movement;
        [SerializeField] private Rigidbody _rigidbody;

        private GameLoopManager _gameLoopManager;
        private GameSettings _gameSettings;

        private float _chargeVelocity;
        private float _rigidbodyDrag;
        private Vector3 _chargePosition;
        private const float PushForce = 6;

        public override void OnStartServer() =>
            _detector.CollisionEnter += OnCollided;

        public void Initialize(GameLoopManager gameLoopManager, GameSettings settings)
        {
            _gameLoopManager = gameLoopManager;
            _gameSettings = settings;
        }

        public void TriggerCharge(Vector3 movementNormalized)
        {
            if (isLocalPlayer)
            {
                Debug.Log("Trigger charge");
                var distance = _gameSettings.ChargeDistance;
                var duration = _gameSettings.ChargingTime;
                _movement.Move(Vector2.zero);
                ChargeLocalClient(movementNormalized, distance, duration);
            }
        }

        private void FixedUpdate()
        {
            if (IsCharging && isLocalPlayer)
                _movement.SetVelocity(ChargingDirection * _chargeVelocity);
        }

        [Client]
        private void ChargeLocalClient(Vector3 forward, float distance, float duration)
        {
            Debug.Log("Charging...");

            IsCharging = true;
            ChargingDirection = forward.normalized;
            _chargePosition = transform.position;

            _chargeVelocity = distance / duration;
            StartCoroutine(DisableCharge(duration));
        }

        [Command]
        private void CmdCompleteChargeRpc() =>
            CompleteChargeRpc();

        [ClientRpc]
        private void CompleteChargeRpc() =>
            Completed?.Invoke(this);

        private IEnumerator DisableCharge(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            _movement.SetVelocity(Vector3.zero);

            var translation = transform.position - _chargePosition;
            Debug.Log($"Travelled distance = {translation.magnitude}, required {_chargeVelocity * duration}");

            IsCharging = false;
            ChargingDirection = Vector3.zero;
            CmdCompleteChargeRpc();
        }

        private void OnCollided(GameObject _, Collision target)
        {
            if (IsCharging == false || !isServer)
                return;

            if (!target.gameObject.TryGetComponent(out CharacterContainer container)) return;

            Debug.Log("[Server]: Player collided with another player while charge");
            if (container.Invincibility.IsInvincible)
            {
                Debug.Log("[Server]: Player cant push invincible player");
                return;
            }

            var pushImpulse = target.GetContact(0).normal * PushForce;
            var invincibilityDuration = _gameSettings.InvincibilityDuration;
            _gameLoopManager.RegisterHit(netIdentity.netId, container.Identity.netId);
            container.Invincibility.ApplyInvincibility(pushImpulse, invincibilityDuration);
        }

        private void OnDestroy() =>
            _detector.CollisionEnter -= OnCollided;
    }
}