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


        public void ApplyInvincibility(float duration)
        {
            if (!isServer) return;

            CmdSetInvincibility(true);
            RpcActivateInvincibility(duration);
        }

        [ClientRpc]
        private void RpcActivateInvincibility(float duration)
        {
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