using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerParry : IPlayerBehaviour
    {
        public UnityEvent OnStartParry = new UnityEvent();
        public UnityEvent OnStopParry = new UnityEvent();
        public UnityEvent OnSuccessfulParry = new UnityEvent();

        private float parryStartTimestamp;
        private float successfulParryTimestamp;
        private float recoveryParryTimestamp;
        private float parryCooldownTimestamp = -1.0f;

        private bool wasSuccessful;
        public bool WasSuccessful => wasSuccessful;
        private bool isInRecovery;

        public bool isFromParry { get; private set; }

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            Debug.Log("PARRY");

            isInRecovery = false;
            wasSuccessful = false;
            parryStartTimestamp = Time.time;
            isFromParry = previous == BehaviourType.Parry;

            player.playerStamina.ConsumeStamina(player.playerData.parryStaminaCost);

            OnStartParry?.Invoke();
        }

        public void TriggerSuccessfulParry(PlayerStateMachine player)
        {
            wasSuccessful = true;
            successfulParryTimestamp = Time.time;
            player.playerStamina.GainStamina(player.playerData.parryStaminaGainOnSuccess);
            OnSuccessfulParry?.Invoke();
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (player.inputPackage.GetParry.WasPressedWithBuffer())
            {
                StartBehaviour(player, BehaviourType.Parry);
                return;
            }

            if (player.playerRoll.CanRoll(player) && player.inputPackage.GetRoll.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerRoll);
                return;
            }

            if (player.playerJump.CanJump(player) && player.inputPackage.GetJump.WasPressedWithBuffer())
            {
                player.ChangeBehaviour(player.playerJump);
                return;
            }


            if (!isInRecovery)
            {
                bool regularParryIsOver = !wasSuccessful && Time.time - parryStartTimestamp >= player.playerData.parryDuration;
                bool successfulParryIsOver = wasSuccessful && Time.time - successfulParryTimestamp >= player.playerData.successfulParryDuration;

                if (regularParryIsOver || successfulParryIsOver)
                {
                    isInRecovery = true;
                    recoveryParryTimestamp = Time.time;
                    OnStopParry?.Invoke();
                }
            }
            else
            {
                bool isRegularRecoveryOver = !wasSuccessful && Time.time - recoveryParryTimestamp >= player.playerData.parryRecoveryDuration;
                bool isSuccessfulRecoveryOver = wasSuccessful && Time.time - recoveryParryTimestamp >= player.playerData.successfulParryRecoveryDuration;

                if (isRegularRecoveryOver || isSuccessfulRecoveryOver)
                    player.ChangeBehaviour(player.playerIdle);
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

        public bool CanParry(PlayerStateMachine player)
        {
            bool isCooldownRefreshed = Time.time >= parryCooldownTimestamp;
            bool hasEnoughStamina = player.playerData.parryStaminaCost == 0.0f || !player.playerStamina.IsEmpty;

            return isCooldownRefreshed && hasEnoughStamina;
        }

        public bool IsParrying(PlayerStateMachine player)
        {


            return player.currentBehaviour.GetBehaviourType() == BehaviourType.Parry && !isInRecovery && (!wasSuccessful || Time.time - successfulParryTimestamp <= player.playerData.parryGracePeriodDuration);
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
            parryCooldownTimestamp = wasSuccessful ? -1.0f : (Time.time + player.playerData.parryCooldown);
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.Parry;
        }
    }
}
