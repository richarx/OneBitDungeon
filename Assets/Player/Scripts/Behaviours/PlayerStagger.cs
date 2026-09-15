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
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.staggerDeceleration * Time.fixedDeltaTime); //* Mathf.Sqrt(Time.fixedDeltaTime));
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.staggerDeceleration * Time.fixedDeltaTime); //* Mathf.Sqrt(Time.fixedDeltaTime));
        }

        public void BounceOffWall(PlayerStateMachine player, Collision collision, float speedRetention)
        {
            Vector3 incomingVelocity = player.moveVelocity;
            incomingVelocity.y = 0.0f;

            if (incomingVelocity.sqrMagnitude <= 0.0001f)
                return;

            Vector3 wallNormal = GetMostOpposingWallNormal(collision, incomingVelocity);
            if (wallNormal.sqrMagnitude <= 0.0001f)
                return;

            Vector3 normalVelocity = Vector3.Project(incomingVelocity, wallNormal);
            Vector3 tangentialVelocity = incomingVelocity - normalVelocity;
            player.moveVelocity = tangentialVelocity - normalVelocity * speedRetention;
            player.SetLastLookDirection((-player.moveVelocity).ToVector2());
            player.ApplyMovement();
        }

        private static Vector3 GetMostOpposingWallNormal(Collision collision, Vector3 incomingVelocity)
        {
            Vector3 incomingDirection = incomingVelocity.normalized;
            Vector3 wallNormal = Vector3.zero;
            float mostOpposingDot = 0.0f;

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 contactNormal = collision.GetContact(i).normal;
                contactNormal.y = 0.0f;

                if (contactNormal.sqrMagnitude <= 0.0001f)
                    continue;

                contactNormal.Normalize();
                float dot = Vector3.Dot(incomingDirection, contactNormal);
                if (dot >= mostOpposingDot)
                    continue;

                mostOpposingDot = dot;
                wallNormal = contactNormal;
            }

            return wallNormal;
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
