using Game.Character.Ability;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Character
{
    public class CharacterInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerInput input;
        [SerializeField] private CharacterMovement movement;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private ChargeAbility chargeAbility;
        [SerializeField] private Transform modelTransform;

        private InputAction _walkAction;
        private InputAction _chargeAction;
        private InputAction _lookAction;

        private void Awake()
        {
            _walkAction = input.actions["Move"];
            _lookAction = input.actions["Look"];
            _chargeAction = input.actions["Charge"];
            _chargeAction.performed += OnChargePerformed;
            chargeAbility.Completed += OnChargeCompleted;
        }


        private void FixedUpdate()
        {
            var walk = _walkAction.ReadValue<Vector2>();
            if (!chargeAbility.IsCharging)
                movement.Move(walk);

            cameraController.RotateCamera(_lookAction.ReadValue<Vector2>());
        }

        private void OnChargePerformed(InputAction.CallbackContext _)
        {
            if (chargeAbility.IsCharging) return;

            chargeAbility.TriggerCharge(modelTransform.forward);
        }

        private void OnChargeCompleted(ChargeAbility _)
        {
            Debug.Log("Charge completed");
        }
    }
}