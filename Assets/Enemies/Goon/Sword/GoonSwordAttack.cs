using Enemies.Scripts.Behaviours;
using Tools_and_Scripts;
using UnityEngine;

namespace Enemies.Goon.Sword
{
    public class GoonSwordAttack : MonoBehaviour, IEnemyBehaviour
    {
        [Space]
        [Header("Attack")] 
        public float attackDuration;
        public float delayBeforeDash;
        public float attackDashDuration;
        public float attackDashMaxDistance;

        [Space] 
        [Header("Dash")] 
        public float dashDuration;
        public float dashMaxDistance;
        public float dashSpeed;
        
        private GoonSwordDashAttack goonSwordDashAttack;
        private GoonSwordDash goonSwordDash;
        private EnemyStrafe enemyStrafe;

        private void Start()
        {
            goonSwordDashAttack = new GoonSwordDashAttack(this);
            goonSwordDash = new GoonSwordDash(this);
            enemyStrafe = new EnemyStrafe();
        }

        private void SelectNextBehaviour(EnemyStateMachine enemy)
        {
            if (enemy.distanceToPlayer > enemy.enemyData.distanceToPlayerWalkThreshold)
                enemy.ChangeBehaviour(enemy.enemyWalk);
            else
            {
                if (!enemy.damageable.IsFullLife && goonSwordDash.CanDash(enemy) && Tools.RandomBool())
                    enemy.ChangeBehaviour(goonSwordDash);
                else if (goonSwordDashAttack.CanAttack())
                {
                    if (enemy.distanceToPlayer <= enemy.enemyData.distanceToPlayerApproachThreshold)
                        enemy.ChangeBehaviour(goonSwordDashAttack);
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
