using UnityEngine;

namespace Player.Scripts
{
    public class Character2JumpTagStrategy : IJumpTagStrategy
    {
        [SerializeField] private float backdashDuration = 0.16f;
        [SerializeField] private float totalDuration = 0.32f;
        [SerializeField] private float backdashSpeed = 14.0f;
        [SerializeField] private float acceleration = 180.0f;
        [SerializeField] private float deceleration = 140.0f;

        private float startTimestamp;
        private Vector3 backdashDirection;

        public void Initialize(PlayerStateMachine player) { }

        public CodeAnimator.AnimationType SelectAnimation()
        {
            return CodeAnimator.AnimationType.Jump;
        }

        public void StartJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag)
        {
            startTimestamp = Time.time;

            Vector2 lookDirection = player.LastLookDirection.sqrMagnitude > 0.001f
                ? player.LastLookDirection.normalized
                : Vector2.right;

            backdashDirection = new Vector3(-lookDirection.x, 0.0f, -lookDirection.y);
            player.moveVelocity = Vector3.zero;
        }

        public void UpdateJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag)
        {
            if (Time.time - startTimestamp < totalDuration)
                return;

            if (player.moveInput.magnitude >= 0.15f)
                player.ChangeBehaviour(player.playerRun);
            else
                player.ChangeBehaviour(player.playerIdle);
        }

        public void FixedUpdateJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag)
        {
            float elapsed = Time.time - startTimestamp;

            if (elapsed <= backdashDuration)
            {
                Vector3 targetVelocity = backdashDirection * backdashSpeed;
                player.moveVelocity.x = Mathf.MoveTowards(player.moveVelocity.x, targetVelocity.x, acceleration * Time.fixedDeltaTime);
                player.moveVelocity.z = Mathf.MoveTowards(player.moveVelocity.z, targetVelocity.z, acceleration * Time.fixedDeltaTime);
            }

            player.ApplyMovement();
        }

        public void StopJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag)
        {
            player.moveVelocity = Vector3.ClampMagnitude(player.moveVelocity, player.playerData.walkMaxSpeed);
        }
    }
}
