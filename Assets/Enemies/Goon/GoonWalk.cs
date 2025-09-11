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
            if (goon.position.magnitude <= 0.5f)
                goon.ChangeBehaviour(goon.goonIdle);
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
            HandleDirection(goon);
            goon.ApplyMovement();
        }
        
        private void HandleDirection(GoonStateMachine goon)
        {
            Vector3 direction = goon.position.normalized * -1.0f;
            Vector3 move = direction * goon.goonData.walkMaxSpeed;
            
            goon.moveVelocity.x = Mathf.MoveTowards(goon.moveVelocity.x, move.x, goon.goonData.groundAcceleration * Time.fixedDeltaTime);
            goon.moveVelocity.z = Mathf.MoveTowards(goon.moveVelocity.z, move.z, goon.goonData.groundAcceleration * Time.fixedDeltaTime);
            goon.lastLookDirection = direction;
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
