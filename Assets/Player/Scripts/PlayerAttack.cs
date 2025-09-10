using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerAttack : IPlayerBehaviour
    {
        public UnityEvent<string> OnPlayerAttack = new UnityEvent<string>();

        private float attackStartTimestamp;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            attackStartTimestamp = Time.time;
            
            player.moveVelocity = Vector3.zero;
            player.ApplyMovement();
            
            OnPlayerAttack?.Invoke("Attack_1");
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
