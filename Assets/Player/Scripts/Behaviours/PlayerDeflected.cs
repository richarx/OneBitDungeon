using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    // A deflected sword stroke has its own recovery; it does not grant damage invulnerability.
    public sealed class PlayerDeflected : IPlayerBehaviour
    {
        public UnityEvent<Vector3> OnDeflected = new UnityEvent<Vector3>();

        private Vector3 _opponentPosition;
        private float _defenceUnlockTime;
        private float _attackUnlockTime = -1.0f;

        public bool IsAttackLocked => Time.time < _attackUnlockTime;

        public void Trigger(PlayerStateMachine player, Vector3 opponentPosition)
        {
            if (!player.isAttacking || player.playerHealth.IsDead)
                return;

            _opponentPosition = opponentPosition;
            player.ChangeBehaviour(this);
        }

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            _defenceUnlockTime = Time.time + player.playerData.DeflectedDefenceRecovery;
            _attackUnlockTime = Time.time + player.playerData.DeflectedAttackRecovery;
            DiscardAttackInputs(player);

            Vector3 direction = _opponentPosition - player.position;
            direction.y = 0.0f;
            if (direction.sqrMagnitude > 0.0001f)
                player.SetLastLookDirection(direction.ToVector2());

            player.moveVelocity = -direction.normalized * player.playerData.DeflectedRecoilSpeed;
            player.ApplyMovement();
            OnDeflected.Invoke(_opponentPosition);
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            if (Time.time < _defenceUnlockTime)
                return;

            // Defences have priority over buffered attacks when recovery ends.
            if (player.inputPackage.GetArroganceMode.isPressed)
            {
                if (player.playerArrogantSpin.CanSpin(player) && player.inputPackage.GetRoll.WasPressedWithBuffer())
                {
                    player.ChangeBehaviour(player.playerArrogantSpin);
                    return;
                }
            }
            else
            {
                if (player.playerParry.CanParry(player) &&
                    (player.inputPackage.GetParry.isPressed || player.inputPackage.GetParry.WasPressedWithBuffer()))
                {
                    player.ChangeBehaviour(player.playerParry);
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

            if (!IsAttackLocked)
                player.ChangeBehaviour(player.inputPackage.GetArroganceMode.isPressed
                    ? (IPlayerBehaviour)player.playerArrogantIdle : player.playerIdle);
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            player.moveVelocity = Vector3.MoveTowards(player.moveVelocity, Vector3.zero,
                player.playerData.DeflectedRecoilDeceleration * Time.fixedDeltaTime);
            player.ApplyMovement();
        }

        public void DiscardAttackInputs(PlayerStateMachine player)
        {
            // Do not replay mash inputs after a roll/spin or at the end of the recoil.
            player.inputPackage.GetAttack.lastPressTimestamp = -1.0f;
            player.inputPackage.GetAttack.wasPressedThisFrame = false;
            player.inputPackage.GetCriticalAttack.lastPressTimestamp = -1.0f;
            player.inputPackage.GetCriticalAttack.wasPressedThisFrame = false;
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
            player.moveVelocity = Vector3.zero;
            player.ApplyMovement();
            // The attack deadline survives an early defensive transition.
        }

        public BehaviourType GetBehaviourType() => BehaviourType.Deflected;
    }
}
