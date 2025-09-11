using UnityEngine;
using UnityEngine.Events;

namespace Enemies.Goon
{
    public class GoonStagger : IGoonBehaviour
    {
        public UnityEvent OnGetStaggered = new UnityEvent();
        
        private float startStaggerTimestamp;
        
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            startStaggerTimestamp = Time.time;
            goon.lastLookDirection = goon.ComputeDirectionToPlayer();
            OnGetStaggered?.Invoke();
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
            if (Time.time - startStaggerTimestamp >= goon.goonData.staggerDuration)
            {
                if (goon.damageable.IsDead)
                    goon.ChangeBehaviour(goon.goonDead);
                else
                    goon.ChangeBehaviour(goon.goonIdle);
                return;
            }
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
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
