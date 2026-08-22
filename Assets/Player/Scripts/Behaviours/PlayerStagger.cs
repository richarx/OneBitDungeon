using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerStagger : IPlayerBehaviour
    {
        public UnityEvent OnStagger = new UnityEvent();
        
        private float startStaggerTimestamp;
        private Vector3 knockBackDirection;
        private float knockBackPower;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            startStaggerTimestamp = Time.time;
            player.SetLastLookDirection((knockBackDirection * -1.0f).ToVector2());

            player.moveVelocity = knockBackDirection * knockBackPower;
            player.ApplyMovement();
            
            if (player.playerHealth.IsDead)
                player.playerDead.OnPlayerDies?.Invoke();

            OnStagger?.Invoke();
        }

        public void TriggerStagger(PlayerStateMachine player, Vector3 direction, float power = -1.0f)
        {
            direction.y = 0.0f;
            knockBackDirection = direction.normalized;
            knockBackPower = power >= 0.0f ? power : player.playerData.staggerPower;
            
            if (player.currentBehaviour.GetBehaviourType() == BehaviourType.Stagger)
                StartBehaviour(player, BehaviourType.Stagger);
            else
                player.ChangeBehaviour(player.playerStagger);
        }
        
        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - startStaggerTimestamp >= player.playerData.staggerDuration)
            {
                if (player.playerHealth.IsDead)
                    player.ChangeBehaviour(player.playerDead);
                else    
                    player.ChangeBehaviour(player.playerIdle);
            }
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            HandleDeceleration(player);
            player.ApplyMovement();
        }

        private void HandleDeceleration(PlayerStateMachine player)
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.staggerDeceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.staggerDeceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Stagger;
        }
    }
}
