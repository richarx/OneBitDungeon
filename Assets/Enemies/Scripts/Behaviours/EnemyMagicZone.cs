using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using Warning_Boxes;

namespace Enemies.Scripts.Behaviours
{
    public class EnemyMagicZone : IEnemyBehaviour
    {
        public class EnemyMagicZoneData
        {
            public float attackRadius;
            public float followDuration;
            public float followSpeed;
            public float damageDuration;
            
            public float anticipationDuration;
            public float attackDuration;

            public GameObject damageTriggerPrefab;
            
            public EnemyMagicZoneData(float attackRadius, float followDuration, float followSpeed, float damageDuration, float anticipationDuration, float attackDuration, GameObject damageTriggerPrefab)
            {
                this.attackRadius = attackRadius;
                this.followDuration = followDuration;
                this.followSpeed = followSpeed;
                this.damageDuration = damageDuration;
                this.anticipationDuration = anticipationDuration;
                this.attackDuration = attackDuration;
                this.damageTriggerPrefab = damageTriggerPrefab;
            }
        }
        private EnemyMagicZoneData data;
        
        public EnemyMagicZone(EnemyMagicZoneData enemyMagicZoneData)
        {
            data = enemyMagicZoneData;
        }

        private Vector3 targetPosition;
        private float startAnticipationTimestamp;
        private float startAttackTimestamp;
        private float attackCooldownTimestamp = -1.0f;

        private bool isAnticipationPhase;

        private CircularWarning warningCircle;
        private GameObject currentHitbox;
        
        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous)
        {
            enemy.moveVelocity = Vector3.zero;
            enemy.ApplyMovement();

            isAnticipationPhase = true;
            startAnticipationTimestamp = Time.time;
            warningCircle = WarningBoxes.instance.SpawnCircularWarning(ComputeTargetPosition(), data.attackRadius, data.anticipationDuration);
            enemy.OnAttack.Invoke("DashAttackAnticipation", Vector2.zero);
        }

        public void UpdateBehaviour(EnemyStateMachine enemy)
        {
            if (isAnticipationPhase && Time.time - startAnticipationTimestamp >= data.anticipationDuration && warningCircle.isFull)
                TriggerAttack(enemy);
            else if (currentHitbox != null && Time.time - startAttackTimestamp >= data.damageDuration)
                RemoveDamageHitbox();
                
            if (Time.time - startAnticipationTimestamp >= data.attackDuration)
                enemy.SelectNextBehaviour();
        }

        private void TriggerAttack(EnemyStateMachine enemy)
        {
            isAnticipationPhase = false;
            startAttackTimestamp = Time.time;
            warningCircle.Trigger();
            enemy.OnAttack?.Invoke("DashAttack", Vector2.zero);
            SpawnDamageHitbox(enemy);
        }

        private Vector2 ComputeTargetPosition()
        {
            return PlayerStateMachine.instance.position.ToVector2();
        }

        public void FixedUpdateBehaviour(EnemyStateMachine enemy)
        {
        }
        
        public bool CanAttack()
        {
            return attackCooldownTimestamp <= 0.0f || Time.time >= attackCooldownTimestamp;
        }

        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next)
        {
            if (isAnticipationPhase && warningCircle != null)
                warningCircle.Cancel();
            
            attackCooldownTimestamp = Time.time + Random.Range(1.5f, 3.0f);
            
            if (currentHitbox != null)
                RemoveDamageHitbox();
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Attack;
        }
        
        private void SpawnDamageHitbox(EnemyStateMachine enemy)
        {
            if (currentHitbox != null)
                RemoveDamageHitbox();

            currentHitbox = Object.Instantiate(data.damageTriggerPrefab, warningCircle.transform.position, Quaternion.identity);
            currentHitbox.transform.GetChild(0).GetComponent<EnemyDealDamage>().OnHitParry.AddListener(enemy.GetParried);
        }

        private void RemoveDamageHitbox()
        {
            if (currentHitbox != null)
            {
                Object.Destroy(currentHitbox);
                currentHitbox = null;
            }
        }
    }
}
