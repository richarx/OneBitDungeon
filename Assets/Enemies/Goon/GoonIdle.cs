using UnityEngine;

namespace Enemies.Goon
{
    public class GoonIdle : IGoonBehaviour
    {
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            Debug.Log("GOON IDLE");
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
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
            return BehaviourType.Idle;
        }
    }
}
