using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerDead : IPlayerBehaviour
    {
        public UnityEvent OnPlayerDies = new UnityEvent();
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            player.moveVelocity = Vector3.zero;
            player.ApplyMovement();
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Dead;
        }
    }
}
