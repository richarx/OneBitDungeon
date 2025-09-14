using UnityEngine;

namespace Player.Scripts
{
    public class PlayerRun : IPlayerBehaviour
    {
        private bool isSkippingFrame;
        public bool IsSkippingFrame => isSkippingFrame;

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            Debug.Log("RUN");

            if (previous == BehaviourType.Locked)
            {
                isSkippingFrame = true;
                player.inputPacker.ResetBuffers();
            }
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (isSkippingFrame)
            {
                isSkippingFrame = false;
                return;
            }

            if (player.inputPackage.GetRoll.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerRoll);
                return;
            }
            
            if (player.playerAttack.CanAttack(player) && player.inputPackage.GetAttack.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerAttack);
                return;
            }

            if (player.inputPackage.GetParry.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerParry);
                return;
            }

            if (player.moveInput.magnitude < 0.15f)
            {
                player.ChangeBehaviour(player.playerIdle);
                return;
            }
            
            player.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            if (CanPlayerControlDirection(player))
                HandleDirection(player);
            
            player.ApplyMovement();
        }

        private bool CanPlayerControlDirection(PlayerStateMachine player)
        {
            return !player.isLocked || player.playerLocked.GetLockState == PlayerLocked.LockState.Dialog;
        }

        private void HandleDirection(PlayerStateMachine player)
        {
            Vector3 move = player.moveInput;
            float speed = ComputeMoveSpeed(player);
            move *= speed;
            
            if (player.moveInput.magnitude <= 0.05f)
            {
                player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
                player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
            }
            else
            {
                player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, move.x, player.playerData.groundAcceleration * Time.fixedDeltaTime);
                player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, move.y, player.playerData.groundAcceleration * Time.fixedDeltaTime);
            }
        }

        private float ComputeMoveSpeed(PlayerStateMachine player)
        {
            if (player.isLocked)
            {
                /*
                if (player.playerLocked.GetLockState == PlayerLocked.LockState.Dialog)
                    return player.playerData.dialogWalkMaxSpeed;
                else
                */
                    return 0.0f;
            } 
            
            return player.playerData.walkMaxSpeed;
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
           
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Run;
        }
    }
}
