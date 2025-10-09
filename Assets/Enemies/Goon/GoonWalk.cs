using UnityEngine;

namespace Enemies.Goon
{
    public class GoonWalk : IGoonBehaviour
    {
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            Debug.Log("GOON WALK");
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
            if (goon.distanceToPlayer <= goon.goonData.distanceToPlayerWalkThreshold)
            {
                goon.SelectNextBehaviour();
                return;
            }
            
            goon.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
            HandleDirection(goon);
            goon.ApplyMovement();
        }
        
        private void HandleDirection(GoonStateMachine goon)
        {
            Vector3 direction = goon.directionToPlayer;
            Vector3 move = direction * goon.goonData.walkMaxSpeed;

            goon.moveVelocity.x = Mathf.MoveTowards(goon.moveVelocity.x, move.x, goon.goonData.groundAcceleration * Time.fixedDeltaTime);
            goon.moveVelocity.z = Mathf.MoveTowards(goon.moveVelocity.z, move.z, goon.goonData.groundAcceleration * Time.fixedDeltaTime);
            
            Debug.Log($"Move : {move} / moveVelocity : {goon.moveVelocity} / velocity : {goon.rb.velocity}");
        }

        public void StopBehaviour(GoonStateMachine goon, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Walk;
        }
    }
}
