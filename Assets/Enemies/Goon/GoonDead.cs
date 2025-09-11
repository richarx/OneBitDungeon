using UnityEngine;

namespace Enemies.Goon
{
    public class GoonDead : IGoonBehaviour
    {
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            goon.moveVelocity = Vector3.zero;
            goon.ApplyMovement();
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
        }

        public void StopBehaviour(GoonStateMachine goon, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Dead;
        }
    }
}
