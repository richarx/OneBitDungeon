using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerLocked : IPlayerBehaviour
    {
        public enum LockState
        {
            Dialog,
            Full,
            Hidden,
            Unlocked
        }

        public static UnityEvent OnLockPlayer = new UnityEvent();
        public static UnityEvent OnUnlockPlayer = new UnityEvent();

        private LockState lockState = LockState.Unlocked;
        public LockState GetLockState => lockState;

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            Debug.Log("LOCKED");

            if (lockState == LockState.Full || lockState == LockState.Hidden)
            {
                player.moveVelocity = Vector3.zero;
                player.ApplyMovement();
            }

            if (lockState == LockState.Hidden)
                player.graphics.SetActive(false);

            OnLockPlayer?.Invoke();
        }

        public void SetLockState(PlayerStateMachine player, LockState newLockState = LockState.Full)
        {
            if (lockState == LockState.Hidden)
                player.graphics.SetActive(true);

            lockState = newLockState;
            player.ChangeBehaviour(player.playerLocked);
        }

        public void UnlockPlayer(PlayerStateMachine player)
        {
            if (player.playerRoll.CanRoll(player) && player.inputPackage.GetRoll.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerRoll);
                return;
            }

            if (player.playerAttack.CanAttack(player) && player.inputPackage.GetAttack.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerAttack);
                return;
            }

            if (player.playerParry.CanParry(player) && player.inputPackage.GetParry.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerParry);
                return;
            }

            if (player.moveInput.magnitude >= 0.15f)
            {
                player.ChangeBehaviour(player.playerRun);
                return;
            }

            player.ChangeBehaviour(player.playerIdle);
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (lockState == LockState.Dialog)
                player.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            if (lockState == LockState.Dialog)
                player.playerRun.FixedUpdateBehaviour(player);
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
            if (lockState == LockState.Hidden)
                player.graphics.SetActive(true);
            OnUnlockPlayer?.Invoke();
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Locked;
        }
    }
}
