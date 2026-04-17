namespace Enemies.Scripts.Behaviours
{
    public interface IEnemyBehaviour
    {
        public void StartBehaviour(EnemyController enemy);
        public void UpdateBehaviour(EnemyController enemy);
        public void FixedUpdateBehaviour(EnemyController enemy);
        public void StopBehaviour(EnemyController enemy);
    }
}