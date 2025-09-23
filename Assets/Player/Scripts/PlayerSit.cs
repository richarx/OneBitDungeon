using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerSit : IPlayerBehaviour
    {
        private bool isLocked;
        
        private float getUpTimestamp = -1.0f;
        public bool IsGettingUp => getUpTimestamp >= 0.0f;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            isLocked = true;
            getUpTimestamp = -1.0f;
            player.SetLastLookDirection(Vector2.left);
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
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
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Sit;
        }
    }
}
