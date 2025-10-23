using UnityEngine;

namespace Enemies.Scripts.Behaviours
{
    public class EnemyDead : IEnemyBehaviour
    {
        private float startDieTimestamp;
        private bool hasSpawnedCorpse;
        
        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            hasSpawnedCorpse = false;
            startDieTimestamp = Time.time;
            
            enemy.moveVelocity = Vector3.zero;
            enemy.ApplyMovement();
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (!hasSpawnedCorpse && Time.time - startDieTimestamp >= 0.5f)
            {
                enemy.SpawnCorpse();
                hasSpawnedCorpse = true;
            }
        }

        public void FixedUpdateBehaviour(EnemyStateMachine enemy)
        {
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Dead;
        }
    }
}
