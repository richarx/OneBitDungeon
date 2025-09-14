using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerParry : IPlayerBehaviour
    {
        public UnityEvent OnParry = new UnityEvent();
        public UnityEvent OnSuccessfulParry = new UnityEvent();
        
        private float parryStartTimestamp;
        private float successfulParryTimestamp;

        private bool wasSuccessful;
        public bool WasSuccessful => wasSuccessful;
        
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            Debug.Log("PARRY");
            
            wasSuccessful = false;
            parryStartTimestamp = Time.time;
            OnParry?.Invoke();
        }
        
        public void TriggerSuccessfulParry()
        {
            wasSuccessful = true;
            successfulParryTimestamp = Time.time;
            OnSuccessfulParry?.Invoke();
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (!wasSuccessful && Time.time - parryStartTimestamp >= player.playerData.parryDuration)
            {
                player.ChangeBehaviour(player.playerIdle);
                return;
            }

            if (wasSuccessful && Time.time - successfulParryTimestamp >= player.playerData.successfulParryDuration)
            {
                player.ChangeBehaviour(player.playerIdle);
                return;
            }
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            HandleDirection(player);
            player.ApplyMovement();
        }

        private void HandleDirection(PlayerStateMachine player)
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Parry;
        }
    }
}
