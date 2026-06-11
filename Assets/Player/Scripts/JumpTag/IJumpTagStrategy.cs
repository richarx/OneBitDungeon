using UnityEngine;

namespace Player.Scripts
{
    public interface IJumpTagStrategy
    {
        void Initialize(PlayerStateMachine player);
        CodeAnimator.AnimationType SelectAnimation();
        void StartJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag);
        void UpdateJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag);
        void FixedUpdateJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag);
        void StopJumpTag(PlayerStateMachine player, PlayerJumpTag jumpTag);
    }
}
