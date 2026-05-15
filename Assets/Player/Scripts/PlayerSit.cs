using Game_Manager;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerSit : IPlayerBehaviour
    {
        public UnityEvent OnStartSittingDown = new UnityEvent();
        public UnityEvent OnStartGettingUp = new UnityEvent();

        private bool isLocked;

        private float sitDownTimestamp = -1.0f;
        private float getUpTimestamp = -1.0f;
        public bool IsGettingUp => getUpTimestamp >= 0.0f;
        public bool isRotating { get; private set; }

        private bool hasTargetBeenSet;
        private bool isLeftDirection;
        private Vector2 targetDirection => isLeftDirection ? Vector2.left : Vector2.right;

        private Sequence rotatingSequence;

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            if (player.playerSword.IsSwordInHand)
                player.playerSword.SwapSword();

            sitDownTimestamp = Time.time;
            getUpTimestamp = -1.0f;
            isRotating = true;

            player.moveVelocity = Vector3.zero;
            player.ApplyMovement();

            if (!hasTargetBeenSet)
                isLeftDirection = player.LastLookDirection.x <= 0.0f;
        }

        public void SitAtBonfire(PlayerStateMachine player, Vector3 position)
        {
            isLeftDirection = (position - player.position).normalized.x <= 0.0;
            hasTargetBeenSet = true;

            player.playerHealth.ResetHealth();
            player.ChangeBehaviour(player.playerSit);
            GameManager.instance.SetRespawnPosition();
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (isRotating)
            {
                if (Vector3.Angle(player.LastLookDirection, targetDirection) <= 15.0f)
                {
                    isRotating = false;
                    OnStartSittingDown?.Invoke();
                }
                else
                {
                    player.SetLastLookDirection(Vector3.Slerp(player.LastLookDirection, targetDirection, Tools.NormalizeValue(Time.time - sitDownTimestamp, 0.0f, 0.5f)));
                }
            }

            if (!isLocked && !IsGettingUp && CheckForInput(player))
                GetUp();

            if (IsGettingUp && Time.time - getUpTimestamp >= 0.5f)
                player.ChangeBehaviour(player.playerIdle);
        }

        private bool CheckForInput(PlayerStateMachine player)
        {
            if (player.inputPackage.GetMove.magnitude > 0.15f)
                return true;

            if (player.inputPackage.GetAttack.wasPressedThisFrame)
                return true;

            if (player.inputPackage.GetParry.wasPressedThisFrame)
                return true;

            if (player.inputPackage.GetRoll.wasPressedThisFrame)
                return true;

            if (player.inputPackage.lastInputType == InputType.Gamepad && player.inputPackage.southButton.wasPressedThisFrame)
                return true;

            return false;
        }

        private void GetUp()
        {
            getUpTimestamp = Time.time;
            OnStartGettingUp?.Invoke();
        }

        public void Lock()
        {
            isLocked = true;
        }

        public void Unlock()
        {
            isLocked = false;
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
            getUpTimestamp = -1.0f;
            hasTargetBeenSet = false;
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Sit;
        }
    }
}
