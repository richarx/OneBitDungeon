namespace Enemies.Goon
{
    public enum BehaviourType 
    {
        Spawn,
        Idle,
        Walk,
        Strafe,
        Dash,
        Attack,
        Approach,
        Stagger,
        Dead
    }
    
    public interface IGoonBehaviour
    {
        public void StartBehaviour(GoonStateMachine goon, BehaviourType previous);
        public void UpdateBehaviour(GoonStateMachine goon);
        public void FixedUpdateBehaviour(GoonStateMachine goon);
        public void StopBehaviour(GoonStateMachine goon, BehaviourType next);
        public BehaviourType GetBehaviourType();
    }
}