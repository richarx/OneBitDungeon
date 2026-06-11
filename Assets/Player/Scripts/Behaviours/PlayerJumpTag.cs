using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerJumpTag : IPlayerBehaviour
    {
        public UnityEvent OnStartJumpTag = new UnityEvent();
        public UnityEvent OnSlamJumpTag = new UnityEvent();
        public UnityEvent OnStopJumpTag = new UnityEvent();
        public UnityEvent OnSpawnDamageBox = new UnityEvent();
        public UnityEvent OnRemoveDamageBox = new UnityEvent();

        private IJumpTagStrategy currentStrategy;

        public PlayerJumpTag(IJumpTagStrategy strategy)
        {
            currentStrategy = strategy;
        }

        public void SetStrategy(IJumpTagStrategy strategy)
        {
            if (strategy == null)
                return;

            currentStrategy = strategy;
            currentStrategy.Initialize(PlayerStateMachine.instance);
        }

        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous)
        {
            currentStrategy.Initialize(player);
            currentStrategy.StartJumpTag(player, this);
            OnStartJumpTag?.Invoke();
        }

        public void UpdateBehaviour(PlayerStateMachine player)
        {
            currentStrategy.UpdateJumpTag(player, this);
        }

        public void FixedUpdateBehaviour(PlayerStateMachine player)
        {
            currentStrategy.FixedUpdateJumpTag(player, this);
        }

        public CodeAnimator.AnimationType SelectAnimation()
        {
            return currentStrategy.SelectAnimation();
        }

        public void StopBehaviour(PlayerStateMachine player, BehaviourType next)
        {
            currentStrategy.StopJumpTag(player, this);
            OnRemoveDamageBox?.Invoke();
            OnStopJumpTag?.Invoke();
        }

        public void TriggerSlam()
        {
            OnSlamJumpTag?.Invoke();
            OnSpawnDamageBox?.Invoke();
        }

        public void EndSlamDamage()
        {
            OnRemoveDamageBox?.Invoke();
        }

        public BehaviourType GetBehaviourType()
        {
            return BehaviourType.JumpTag;
        }
    }
}
