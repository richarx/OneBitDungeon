using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerStagger : IPlayerBehaviour
    {
        public UnityEvent OnStagger = new UnityEvent();
        
        private float startStaggerTimestamp;
        private Vector3 knockBackDirection;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            startStaggerTimestamp = Time.time;
            player.SetLastLookDirection((knockBackDirection * -1.0f).ToVector2());
            
            OnStagger?.Invoke();
        }

        public void TriggerStagger(PlayerStateMachine player, Vector3 direction)
        {
            knockBackDirection = direction;
            player.ChangeBehaviour(player.playerStagger);
        }
        
        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time - startStaggerTimestamp >= player.playerData.staggerDuration)
                player.ChangeBehaviour(player.playerIdle);
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Stagger;
        }
    }
}
