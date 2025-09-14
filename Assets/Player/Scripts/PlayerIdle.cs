using UnityEngine;

namespace Player.Scripts
{
    public class PlayerIdle : IPlayerBehaviour
    {
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
//            Debug.Log("IDLE");
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
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

            if (player.moveInput.magnitude >= 0.15f)
            {
                player.ChangeBehaviour(player.playerRun);
                return;
            }
            
            player.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            SlowDownPlayer(player);
            player.ApplyMovement();
        }

        private void SlowDownPlayer(PlayerStateMachine player)
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
           
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Idle;
        }
    }
}
