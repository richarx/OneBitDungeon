using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerParry : IPlayerBehaviour
    {
        public UnityEvent OnStartParry = new UnityEvent();
        public UnityEvent OnStopParry = new UnityEvent();
        public UnityEvent OnSuccessfulParry = new UnityEvent();
        public UnityEvent OnSuccessfulBlock = new UnityEvent();

        private float startParryTimestamp;
        private float successfulParryTimestamp;
        private float recoveryParryTimestamp;

        private bool isInRecovery;

        public bool isFromParry { get; private set; }
        public bool isWalking { get; private set; }

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            isInRecovery = false;
            startParryTimestamp = Time.time;
            successfulParryTimestamp = -1.0f;
            isFromParry = previous == BehaviourType.Parry;

            player.ComputeLastLookDirection();

            OnStartParry?.Invoke();
        }

        public void TriggerSuccessfulParry(PlayerStateMachine player)
        {
            if (Time.time - startParryTimestamp <= player.playerData.parryWindow)
            {
                successfulParryTimestamp = Time.time;
                ArroganceGainEvents.RequestGain(new ArroganceGainRequest(player.playerData.arroganceGainOnParry, ArroganceGainReason.Parry));
                OnSuccessfulParry?.Invoke();
            }
            else
            {
                OnSuccessfulBlock?.Invoke();
            }
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (isInRecovery)
            {
                if (player.inputPackage.GetParry.WasPressedWithBuffer(0.2f))
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
            }

            if (Time.time - successfulParryTimestamp <= player.playerData.parryCounterWindow && player.inputPackage.GetAttack.wasPressedThisFrame)
            {
                player.ChangeBehaviour(player.playerCounterAttack);
                return;
            }

            if (!isInRecovery)
            {
                if (Time.time - startParryTimestamp >= 0.2f && !player.inputPackage.GetParry.isPressed)
                {
                    isInRecovery = true;
                    recoveryParryTimestamp = Time.time;
                    OnStopParry?.Invoke();
                }
            }
            else
            {
                bool isRecoveryOver = Time.time - recoveryParryTimestamp >= player.playerData.parryRecoveryDuration;

                if (isRecoveryOver)
                {
                    player.ChangeBehaviour(player.playerIdle);
                    return;
                }
            }

            player.ComputeLastLookDirection();
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            HandleDirection(player);
            player.ApplyMovement();
        }

        private void HandleDirection(PlayerStateMachine player)
        {
            Vector3 move = player.moveInput;
            float speed = player.playerData.parryWalkSpeed;
            move *= speed;

            isWalking = player.moveInput.magnitude > 0.05f;

            if (isWalking)
            {
                player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, move.x, player.playerData.groundAcceleration * Time.fixedDeltaTime);
                player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, move.y, player.playerData.groundAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
                player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, player.playerData.groundDeceleration * Time.fixedDeltaTime);
            }
        }

        public bool CanParry(PlayerStateMachine player)
        {
            return true;
        }

        public bool IsParrying(PlayerStateMachine player)
        {
            return player.currentBehaviour.GetBehaviourType() == BehaviourType.Parry && !isInRecovery;
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
