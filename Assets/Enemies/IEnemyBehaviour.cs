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
    
    public interface IEnemyBehaviour
    {
        public void StartBehaviour(EnemyStateMachine enemy, BehaviourType previous);
        public void UpdateBehaviour(EnemyStateMachine enemy);
        public void FixedUpdateBehaviour(EnemyStateMachine enemy);
        public void StopBehaviour(EnemyStateMachine enemy, BehaviourType next);
        public BehaviourType GetBehaviourType();
    }
}