using Enemies.Scripts.Behaviours;
using UnityEngine;

namespace Enemies.Goon.MagicZoner
{
    public class GoonMagicZoneAttack : MonoBehaviour, IEnemyBehaviour
    {

        private EnemyFlee enemyFlee;
        
        private void Start()
        {
            enemyFlee = new EnemyFlee();
        }

        private void SelectNextBehaviour(EnemyStateMachine enemy)
        {
            if (enemy.distanceToPlayer > enemy.enemyData.distanceToPlayerWalkThreshold)
                enemy.ChangeBehaviour(enemy.enemyWalk);
            else
            {
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
