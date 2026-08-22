namespace Enemies.Scripts.Behaviours
{
    public sealed class BehaviourExecution
    {
        public static readonly BehaviourExecution Uncontrolled = new BehaviourExecution();

        public EnemyController Controller { get; }
        public IEnemyBehaviour Behaviour { get; }
        public int Id { get; }

        public bool DebugMode => Controller?.DebugMode ?? false;

        private BehaviourExecution()
        {
        }

        public BehaviourExecution(EnemyController controller, IEnemyBehaviour behaviour, int id)
        {
            Controller = controller;
            Behaviour = behaviour;
            Id = id;
        }

        public void Complete()
        {
            Controller?.TryCompleteBehaviour(this);
        }
    }
}
