using Mirror;
using Script.Game.Character.Ability;
using Script.Lobby;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Script.Game.Character
{
    public class CharacterContainer : NetworkBehaviour
    {
        [SyncVar] public LobbyPlayer lobbyPlayer;

        [field: SerializeField] public NetworkIdentity Identity { get; private set; }
        [field: SerializeField] public NetworkTransformReliable Transform { get; private set; }
        [field: SerializeField] public Transform ScorePosition { get; private set; }
        [field: SerializeField] public PlayerInput Input { get; private set; }
        [field: SerializeField] public CharacterInputHandler Character { get; private set; }
        [field: SerializeField] public CameraController CameraController { get; private set; }
        [field: SerializeField] public CharacterMovement Movement { get; private set; }
        [field: SerializeField] public CharacterRotation Rotation { get; private set; }
        [field: SerializeField] public Invincibility Invincibility { get; private set; }
        [field: SerializeField] public ChargeAbility ChargeAbility { get; private set; }

        private AdvanceNetworkManager _advanceNetworkManager;

        public override void OnStartClient()
        {
            _advanceNetworkManager = NetworkManager.singleton as AdvanceNetworkManager;
            if (_advanceNetworkManager != null)
                _advanceNetworkManager.SetupCharacter(this);
        }
    }
}