using UnityEngine;
using UnityEngine.Events;

namespace Enemies.Goon
{
    public class GoonStagger : IGoonBehaviour
    {
        public UnityEvent OnGetStaggered = new UnityEvent();

        private Vector3 staggerDirection;
        private float startStaggerTimestamp;
        private bool isStun;
        
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            Debug.Log("GOON STAGGER");
            
            startStaggerTimestamp = Time.time;
            Vector3 playerDirection = goon.ComputeDirectionToPlayer();
            playerDirection.y = 0.0f;
            
            goon.lastLookDirection = playerDirection.normalized;
            staggerDirection = playerDirection.normalized * -1.0f;
            
            isStun = true;
            
            OnGetStaggered?.Invoke();
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
            if (isStun && Time.time - startStaggerTimestamp >= goon.goonData.stunDuration)
            {
                goon.moveVelocity = staggerDirection * goon.goonData.knockbackPower;
                goon.ApplyMovement();
                isStun = false;
            }
            
            if (Time.time - startStaggerTimestamp >= goon.goonData.staggerDuration)
            {
                if (goon.damageable.IsDead)
                    goon.ChangeBehaviour(goon.goonDead);
                else
                    goon.SelectNextBehaviour();
                return;
            }
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
            HandleDirection(goon);
            goon.ApplyMovement();
        }
        
        private void HandleDirection(GoonStateMachine goon)
        {
            goon.moveVelocity.x = Mathf.MoveTowards(goon.moveVelocity.x, 0.0f, goon.goonData.groundDeceleration * Time.fixedDeltaTime);
            goon.moveVelocity.z = Mathf.MoveTowards(goon.moveVelocity.z, 0.0f, goon.goonData.groundDeceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(GoonStateMachine goon, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Stagger;
        }
    }
}
