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
            Unlocked
        }
        
        public UnityEvent OnLockPlayer = new UnityEvent();
        public UnityEvent OnUnlockPlayer = new UnityEvent();

        private LockState lockState = LockState.Unlocked;
        public LockState GetLockState => lockState;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            Debug.Log("LOCKED");
            
            if (lockState == LockState.Full)
            {
                player.moveVelocity = Vector3.zero;
                player.ApplyMovement();
            }
            
            OnLockPlayer?.Invoke();
        }

        public void SetLockState(PlayerStateMachine player, LockState newLockState = LockState.Full)
        {
            lockState = newLockState;
            player.ChangeBehaviour(player.playerLocked);
        }

        public void SetLockState(LockState newLockState)
        {
            lockState = newLockState;
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            if (lockState == LockState.Dialog)
                player.playerRun.FixedUpdateBehaviour(player);
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {          
            OnUnlockPlayer?.Invoke();
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Locked;
        }
    }
}
