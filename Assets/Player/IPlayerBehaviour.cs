namespace Player
{
    public enum BehaviourType 
    {
        Run,
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
