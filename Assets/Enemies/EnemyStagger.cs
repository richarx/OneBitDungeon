using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies.Goon
{
    public class EnemyStagger : IEnemyBehaviour
    {
        public UnityEvent OnGetStaggered = new UnityEvent();

        private Vector3 staggerDirection;
        private float startStaggerTimestamp;
        private bool isStun;
        
        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            Debug.Log("GOON STAGGER");
            
            startStaggerTimestamp = Time.time;
            
            enemy.SetLastLookDirection(enemy.directionToPlayer.ToVector2());
            staggerDirection = enemy.directionToPlayer * -1.0f;
            
            isStun = true;
            
            OnGetStaggered?.Invoke();
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (isStun && Time.time - startStaggerTimestamp >= enemy.enemyData.stunDuration)
            {
                enemy.moveVelocity = staggerDirection * enemy.enemyData.knockbackPower;
                enemy.ApplyMovement();
                isStun = false;
            }
            
            if (Time.time - startStaggerTimestamp >= enemy.enemyData.staggerDuration)
            {
                if (enemy.damageable.IsDead)
                    enemy.ChangeBehaviour(enemy.enemyDead);
                else
                    enemy.SelectNextBehaviour();
                return;
            }
        }

        public void FixedUpdateBehaviour(EnemyStateMachine enemy)
        {
            HandleDirection(enemy);
            enemy.ApplyMovement();
        }
        
        private void HandleDirection(EnemyStateMachine enemy)
        {
            enemy.moveVelocity.x = Mathf.MoveTowards(enemy.moveVelocity.x, 0.0f, enemy.enemyData.groundDeceleration * Time.fixedDeltaTime);
            enemy.moveVelocity.z = Mathf.MoveTowards(enemy.moveVelocity.z, 0.0f, enemy.enemyData.groundDeceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Stagger;
        }
    }
}
