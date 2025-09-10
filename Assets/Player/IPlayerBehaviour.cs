namespace Player
{
    public enum BehaviourType 
    {
        Idle,
        Run,
        Roll,
        Attack,
        Locked,
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
