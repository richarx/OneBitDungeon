using UnityEngine;

namespace Player.Scripts
{
    public class Character1JumpTagStrategy : IJumpTagStrategy
    {
        [SerializeField] private float slamDelay = 0.08f;
        [SerializeField] private float hitboxDuration = 0.12f;
        [SerializeField] private float totalDuration = 0.35f;
        [SerializeField] private float deceleration = 200.0f;

        private float startTimestamp;
        private bool hasSlammed;
        private bool hasRemovedHitbox;

        public void Initialize(PlayerStateMachine player) { }

        public CodeAnimator.AnimationType SelectAnimation()
        {
            return CodeAnimator.AnimationType.JumpTag;
        }

        public void StartJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag)
        {
            startTimestamp = Time.time;
            hasSlammed = false;
            hasRemovedHitbox = false;

            player.moveVelocity = Vector3.zero;
            player.playerSword.ForceSwordInHand();
        }

        public void UpdateJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag)
        {
            float elapsed = Time.time - startTimestamp;

            if (!hasSlammed && elapsed >= slamDelay)
            {
                hasSlammed = true;
                jumpTag.TriggerSlam();
            }

            if (!hasRemovedHitbox && elapsed >= slamDelay + hitboxDuration)
            {
                hasRemovedHitbox = true;
                jumpTag.EndSlamDamage();
            }

            if (elapsed >= totalDuration)
            {
                if (player.moveInput.magnitude >= 0.15f)
                    player.ChangeBehaviour(player.playerRun);
                else
                    player.ChangeBehaviour(player.playerIdle);
            }
        }

        public void FixedUpdateJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag)
        {
            player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, 0.0f, deceleration * Time.fixedDeltaTime);
            player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, 0.0f, deceleration * Time.fixedDeltaTime);
            player.ApplyMovement();
        }

        public void StopJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag) { }
    }
}
