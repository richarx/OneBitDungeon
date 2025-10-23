using Enemies.Scripts.Behaviours;
using Tools_and_Scripts;
using UnityEngine;

namespace Enemies.Goon.Sword
{
    public class GoonSwordAttack : MonoBehaviour, IEnemyBehaviour
    {
        private EnemyDashAttack enemyDashAttack;
        private EnemyDash enemyDash;
        private EnemyStrafe enemyStrafe;

        private void Start()
        {
            enemyDashAttack = new EnemyDashAttack(new EnemyDashAttack.EnemyDashAttackData(1.3f, 0.5f, 0.15f, 10.0f));
            enemyDash = new EnemyDash(new EnemyDash.EnemyDashData(0.5f, 8.0f, 0.3f));
            enemyStrafe = new EnemyStrafe();
        }

        private void SelectNextBehaviour(EnemyStateMachine enemy)
        {
            if (enemy.distanceToPlayer > enemy.enemyData.distanceToPlayerWalkThreshold)
                enemy.ChangeBehaviour(enemy.enemyWalk);
            else
            {
                if (!enemy.damageable.IsFullLife && enemyDash.CanDash(enemy) && Tools.RandomBool())
                    enemy.ChangeBehaviour(enemyDash);
                else if (enemyDashAttack.CanAttack())
                {
                    if (enemy.distanceToPlayer <= enemy.enemyData.distanceToPlayerApproachThreshold)
                        enemy.ChangeBehaviour(enemyDashAttack);
                    else
                        enemy.ChangeBehaviour(enemy.enemyApproach);
                }
                else
                    enemy.ChangeBehaviour(enemyStrafe);
            }
        }

        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            SelectNextBehaviour(enemy);
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
        }

        public void FixedUpdateBehaviour(EnemyStateMachine enemy)
        {
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Attack;
        }
    }
}
