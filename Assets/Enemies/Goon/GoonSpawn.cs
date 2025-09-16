using UnityEngine;

namespace Enemies.Goon
{
    public class GoonSpawn : IGoonBehaviour
    {
        private float spawnTimestamp;
        private float spawnDelay;

        public bool isLocked => Time.time - spawnTimestamp <= spawnDelay;
        
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            Debug.Log("GOON SPAWN");
            
            spawnTimestamp = Time.time;
            spawnDelay = goon.goonData.spawnDelay;

            goon.moveVelocity = Vector3.zero;
            goon.ApplyMovement();
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
            if (Time.time - spawnTimestamp >= goon.goonData.spawnWalkDuration)
            {
                goon.ChangeBehaviour(goon.goonIdle);
                return;
            }
            
            goon.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
            if (!isLocked)
            {
                HandleDirection(goon);
                goon.ApplyMovement();
            }
        }
        
        private void HandleDirection(GoonStateMachine goon)
        {
            Vector3 direction = goon.position.normalized * -1.0f;
            Vector3 move = direction * goon.goonData.walkMaxSpeed;
            
            goon.moveVelocity.x = Mathf.MoveTowards(goon.moveVelocity.x, move.x, goon.goonData.groundAcceleration * Time.fixedDeltaTime);
            goon.moveVelocity.z = Mathf.MoveTowards(goon.moveVelocity.z, move.z, goon.goonData.groundAcceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(GoonStateMachine goon, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Spawn;
        }
    }
}
