using UnityEngine;
using UnityEngine.Events;

namespace Enemies.Goon
{
    public class GoonDash : IGoonBehaviour
    {
        public UnityEvent OnGoonDash = new UnityEvent();
        
        private float startDashTimestamp;
        private float dashCooldownTimestamp;
        
        private Vector3 dashStartPosition;
        private Vector3 dashTarget;
        private Vector3 dashVelocity;

        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            Debug.Log("GOON DASH");

            dashStartPosition = goon.position;
            ComputeDashTarget(goon);
            startDashTimestamp = Time.time;
            
            OnGoonDash?.Invoke();
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
            if (Time.time - startDashTimestamp >= goon.goonData.dashDuration)
            {
                goon.SelectNextBehaviour();
                return;
            }
            
            goon.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
            if (Vector3.Distance(dashStartPosition, goon.position) <= Vector3.Distance(dashStartPosition, dashTarget))
                DashTowardTarget(goon);
            else
                goon.moveVelocity = Vector3.zero;

            goon.ApplyMovement();
        }
        
        private void ComputeDashTarget(GoonStateMachine goon)
        {
            dashTarget = goon.position + goon.directionToPlayer * (-1.0f * goon.goonData.dashMaxDistance);
        }
        
        private void DashTowardTarget(GoonStateMachine goon)
        {
            goon.rb.MovePosition(Vector3.SmoothDamp(goon.position, dashTarget, ref dashVelocity, goon.goonData.dashSpeed));
        }

        public bool CanDash(GoonStateMachine goon)
        {
            return goon.currentBehaviour.GetBehaviourType() != BehaviourType.Dash && (dashCooldownTimestamp <= 0.0f || Time.time >= dashCooldownTimestamp);
        }

        public void StopBehaviour(GoonStateMachine goon, BehaviourType next)
        {
            dashCooldownTimestamp = Time.time + Random.Range(1.5f, 3.0f);
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Dash;
        }
    }
}
