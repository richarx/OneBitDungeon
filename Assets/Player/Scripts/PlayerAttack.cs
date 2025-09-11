using System.Collections.Generic;
using Enemies;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerAttack : IPlayerBehaviour
    {
        public UnityEvent<string> OnPlayerAttack = new UnityEvent<string>();

        private Vector3 dashTarget;
        private string currentAttack;
        private float attackStartTimestamp;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            attackStartTimestamp = Time.time;

            dashTarget = player.position;
            ComputeDashTarget(player);
            
            SelectCurrentAttack(player);
            
            OnPlayerAttack?.Invoke(currentAttack);
        }

        private void ComputeDashTarget(PlayerStateMachine player)
        {
            Vector3 position = player.position;
            Vector2 inputDirection = player.moveInput.magnitude >= 0.15f ? player.moveInput : player.lastLookDirection;
            float inputAngle = inputDirection.normalized.ToDegree();

            List<GameObject> enemies = EnemyHolder.instance.Enemies;
            List<Vector3> positionsInRange = new List<Vector3>();

            foreach (GameObject enemy in enemies)
            {
                Vector3 directionToEnemy = (enemy.transform.position - position);
                float distanceToEnemy = directionToEnemy.magnitude;
                directionToEnemy = directionToEnemy.normalized;
                
                float angleToEnemy = new Vector2(directionToEnemy.x, directionToEnemy.z).ToDegree();

                if (distanceToEnemy <= 1.0f || (distanceToEnemy < player.playerData.attackDashMaxDistance && angleToEnemy <= inputAngle + 90.0f && angleToEnemy >= inputAngle - 90.0f))
                    positionsInRange.Add(enemy.transform.position);
            }

            if (positionsInRange.Count < 1)
            {
                player.lastLookDirection = inputDirection.normalized;
                return;
            }
            
            float minDistance = player.playerData.attackDashMaxDistance;
            int closestEnemy = 0;

            for (int i = 0; i < positionsInRange.Count; i++)
            {
                Vector3 directionToEnemy = (positionsInRange[i] - position);
                float distanceToEnemy = directionToEnemy.magnitude;

                if (distanceToEnemy < minDistance)
                {
                    minDistance = distanceToEnemy;
                    closestEnemy = i;
                }
            }

            dashTarget = positionsInRange[closestEnemy];
            Vector3 direction = (dashTarget - position).normalized;
            player.lastLookDirection = new Vector2(direction.x, direction.z);
        }

        private void SelectCurrentAttack(PlayerStateMachine player)
        {
            currentAttack = "Attack_1";
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - attackStartTimestamp >= player.playerData.attackDuration)
            {
                player.ChangeBehaviour(player.playerIdle);
                return;
            }
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - attackStartTimestamp <= player.playerData.attackDashDuration && Vector3.Distance(player.position, dashTarget) >= 1.0f)
                DashTowardTarget(player);
            else
                Decelerate(player);

            player.ApplyMovement();
        }

        private void Decelerate(PlayerStateMachine player)
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.attackDashDeceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.attackDashDeceleration * Time.fixedDeltaTime);
        }

        private void DashTowardTarget(PlayerStateMachine player)
        {
            Vector3 direction = (dashTarget - player.position).normalized * player.playerData.attackDashSpeed;
            
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, direction.x, player.playerData.attackDashAcceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, direction.z, player.playerData.attackDashAcceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
        }

        public bool CanAttack(PlayerStateMachine player)
        {
            return player.playerSword.CurrentlyHasSword;
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Attack;
        }
    }
}
