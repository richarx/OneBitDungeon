using UnityEngine;

namespace Player
{
    public class PlayerIdle : IPlayerBehaviour
    {
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            Debug.Log("IDLE");
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (player.inputPackage.GetRoll.wasPressedThisFrame)
            {
                player.ChangeBehaviour(player.playerRoll);
                return;
            }

            if (player.moveInput.magnitude >= 0.15f)
            {
                player.ChangeBehaviour(player.playerRun);
                return;
            }
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
