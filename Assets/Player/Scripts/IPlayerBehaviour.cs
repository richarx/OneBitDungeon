namespace Player.Scripts
{
    public enum BehaviourType 
    {
        Idle,
        Run,
        Roll,
        Attack,
        Stagger,
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
