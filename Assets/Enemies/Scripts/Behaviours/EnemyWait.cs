using UnityEngine;

namespace Enemies.Scripts.Behaviours
{
    public class EnemyWait : IEnemyBehaviour
    {
        private float endWaitTimestamp;
        
        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            enemy.moveVelocity = Vector3.zero;
            enemy.ApplyMovement();
            
            endWaitTimestamp = Time.time + Random.Range(0.5f, 1.5f);
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (Time.time >= endWaitTimestamp)
            {
                enemy.SelectNextBehaviour();
                return;
            }
            
            enemy.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(EnemyStateMachine enemy)
        {
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Idle;
        }
    }
}
