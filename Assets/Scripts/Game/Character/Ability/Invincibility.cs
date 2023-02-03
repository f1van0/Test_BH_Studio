using System.Collections;
using Mirror;
using UnityEngine;

namespace Game.Character.Ability
{
    public class Invincibility : NetworkBehaviour
    {
        [SerializeField] private MeshRenderer modelRenderer;
        [SerializeField] private CharacterMovement movement;

        [field: SyncVar] public bool IsInvincible { get; private set; }


        public void ApplyInvincibility(Vector3 pushImpulse, float duration)
        {
            if (!isServer) return;

            CmdSetInvincibility(true);
            RpcActivateInvincibility(pushImpulse, duration);
        }

        [ClientRpc]
        private void RpcActivateInvincibility(Vector3 pushImpulse, float duration)
        {
            if (isLocalPlayer)
                movement.AddImpulse(pushImpulse);

            StartCoroutine(ShowInvincibleState(duration));
        }

        [Command(requiresAuthority = false)]
        private void CmdSetInvincibility(bool value)
        {
            IsInvincible = value;
        }

        private IEnumerator ShowInvincibleState(float duration)
        {
            var material = modelRenderer.material;
            var oldColor = material.color;

            material.color = Color.red;
            yield return new WaitForSeconds(duration);
            material.color = oldColor;

            if (isServer)
                CmdSetInvincibility(false);
        }
    }
}