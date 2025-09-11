using Tools_and_Scripts;
using UnityEngine;

namespace Enemies.Goon
{
    public class GoonDead : IGoonBehaviour
    {
        private float startDieTimestamp;
        private bool hasSpawnedCorpse;
        
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous)
        {
            hasSpawnedCorpse = false;
            startDieTimestamp = Time.time;
            
            goon.moveVelocity = Vector3.zero;
            goon.ApplyMovement();
        }

        public void UpdateBehaviour(GoonStateMachine goon)
        {
            if (!hasSpawnedCorpse && Time.time - startDieTimestamp >= 0.5f)
            {
                goon.SpawnCorpse();
                hasSpawnedCorpse = true;
            }
        }

        public void FixedUpdateBehaviour(GoonStateMachine goon)
        {
        }

        public void StopBehaviour(GoonStateMachine goon, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Dead;
        }
    }
}
