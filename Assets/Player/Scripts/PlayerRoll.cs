using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerRoll : IPlayerBehaviour
    {
        public UnityEvent OnStartRoll = new UnityEvent();
        public UnityEvent OnStopRoll = new UnityEvent();
        
        private Vector3 rollDirection;
        private Vector3 rollStartPosition;
        private float rollStartTimestamp;
        private float rollCooldownTimestamp = -1.0f;

        public bool IsRollingLeft => rollDirection.x >= 0.0f;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            Debug.Log("Roll");
            
            Vector2 inputDirection = player.moveInput.magnitude >= 0.15f ? player.moveInput.normalized : player.LastLookDirection;
            player.SetLastLookDirection(inputDirection);
            rollDirection = inputDirection.ToVector3();
            rollStartPosition = player.position;
            rollStartTimestamp = Time.time;
            
            player.playerStamina.ConsumeStamina(player.playerData.rollStaminaCost);

            Warning_Boxes.WarningBoxes.instance.SpawnCircularWarning(player.position.ToVector2(), 2.0f, 1.0f);
            
            OnStartRoll?.Invoke();
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
        
        public bool CanRoll(PlayerStateMachine player)
        {
            return (rollCooldownTimestamp < 0.0f || Time.time >= rollCooldownTimestamp) && !player.playerStamina.IsEmpty;
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
            rollCooldownTimestamp = Time.time + player.playerData.rollCooldown;
            player.moveVelocity = Vector3.ClampMagnitude(player.moveVelocity, player.playerData.walkMaxSpeed);
            OnStopRoll?.Invoke();
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Roll;
        }
    }
}
