using Enemies.Scripts.Behaviours;
using UnityEngine;

namespace Enemies.Goon.MagicZoner
{
    public class GoonMagicZoneAttack : MonoBehaviour, IEnemyBehaviour
    {
        [SerializeField] private GameObject damageHitBoxPrefab;
        
        private EnemyFlee enemyFlee;
        private EnemyWait enemyWait;
        private EnemyMagicZone enemyMagicZone;
        
        private void Start()
        {
            enemyFlee = new EnemyFlee();
            enemyWait = new EnemyWait();
            enemyMagicZone = new EnemyMagicZone(new EnemyMagicZone.EnemyMagicZoneData(2.0f, 1.0f, 3.0f, 0.05f, 1.0f, 2.0f, damageHitBoxPrefab));
        }

        private void SelectNextBehaviour(EnemyStateMachine enemy)
        {
            if (enemy.distanceToPlayer > enemy.enemyData.distanceToPlayerWalkThreshold)
                enemy.ChangeBehaviour(enemy.enemyWalk);
            else 
            {
                if (enemyMagicZone.CanAttack())
                    enemy.ChangeBehaviour(enemyMagicZone);
                else if (enemy.damageable.IsFullLife)
                    enemy.ChangeBehaviour(enemyWait);
                else
                    enemy.ChangeBehaviour(enemyFlee);
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
