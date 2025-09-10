using UnityEngine;

namespace Player
{
    public class PlayerRoll : IPlayerBehaviour
    {
        private Vector3 rollDirection;
        private Vector3 rollStartPosition;
        private float rollStartTimestamp;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            Debug.Log("Roll");
            
            Vector2 inputDirection = player.moveInput.magnitude >= 0.15f ? player.moveInput : player.lastLookDirection;
            rollDirection = new Vector3(inputDirection.x, 0.0f, inputDirection.y).normalized;
            rollStartPosition = player.position;
            rollStartTimestamp = Time.time;
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - rollStartTimestamp >= player.playerData.rollMaxDuration)
            {
                StopRoll(player);
                return;
            }

            if (Vector3.Distance(rollStartPosition, player.position) >= player.playerData.rollMaxDistance)
            {
                StopRoll(player);
                return;
            }
        }

        private void StopRoll(PlayerStateMachine player)
        {
            if (player.moveInput.magnitude >= 0.15f)
                player.ChangeBehaviour(player.playerRun);
            else
                player.ChangeBehaviour(player.playerIdle);
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            HandleAcceleration(player);
            player.ApplyMovement();
        }

        private void HandleAcceleration(PlayerStateMachine player)
        {
            Vector3 move = rollDirection * player.playerData.rollMaxSpeed;
            float distanceMoved = Vector3.Distance(rollStartPosition, player.position);
            float normalizedDistance = Tools.NormalizeValue(distanceMoved, 0.0f, player.playerData.rollMaxDistance);
            
            if (normalizedDistance >= player.playerData.rollDecelerationDistanceThreshold)
            {
                player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.rollDeceleration * Time.fixedDeltaTime);
                player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.rollDeceleration * Time.fixedDeltaTime);
            }
            else
            {
                player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, move.x, player.playerData.rollAcceleration * Time.fixedDeltaTime);
                player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, move.z, player.playerData.rollAcceleration * Time.fixedDeltaTime);
            }
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
            player.moveVelocity = Vector3.ClampMagnitude(player.moveVelocity, player.playerData.walkMaxSpeed);
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Roll;
        }
    }
}
