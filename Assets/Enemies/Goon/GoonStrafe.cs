using Tools_and_Scripts;
using UnityEngine;

namespace Enemies.Goon
{
    public class GoonStrafe : IGoonBehaviour
    {
        private bool isGoingLeft;
        private float endStrafeTimestamp;
        
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            Debug.Log("GOON STRAFE");
            
            isGoingLeft = Tools.RandomBool();
            endStrafeTimestamp = Time.time + Random.Range(0.5f, 1.5f);
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
            if (Time.time >= endStrafeTimestamp)
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
            Vector3 direction = goon.directionToPlayer.ToVector2().AddAngleToDirection(isGoingLeft ? 90.0f : -90.0f).ToVector3();
            Vector3 move = direction * goon.goonData.walkMaxSpeed;
            
            goon.moveVelocity.x = Mathf.MoveTowards(goon.moveVelocity.x, move.x, goon.goonData.groundAcceleration * Time.fixedDeltaTime);
            goon.moveVelocity.z = Mathf.MoveTowards(goon.moveVelocity.z, move.z, goon.goonData.groundAcceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(GoonStateMachine goon, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Strafe;
        }
    }
}
