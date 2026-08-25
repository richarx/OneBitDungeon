namespace Player.Scripts
{
    public enum BehaviourType
    {
        Idle,
        ArrogantIdle,
        Run,
        ArrogantRun,
        Roll,
        ArrogantSpin,
        Jump,
        JumpTag,
        Attack,
        CriticalAttack,
        Stagger,
        Parry,
        Sit,
        Dead,
        Locked,
        Tag,
        JumpAttack
    }

    public interface IPlayerBehaviour
    {
        public void StartBehaviour(PlayerStateMachine player, BehaviourType previous);
        public void UpdateBehaviour(PlayerStateMachine player);
        public void FixedUpdateBehaviour(PlayerStateMachine player);
        public void StopBehaviour(PlayerStateMachine player, BehaviourType next);
        public BehaviourType GetBehaviourType();
    }
}
