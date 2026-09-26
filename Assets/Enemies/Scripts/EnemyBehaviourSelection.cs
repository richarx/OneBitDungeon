namespace Enemies.Scripts.Behaviours
{
    /// <summary>
    /// Optional context-aware selection contract. Existing behaviours keep the
    /// controller defaults: eligible and weight 100.
    /// </summary>
    public interface IContextualEnemyBehaviour
    {
        bool CanExecute(EnemyContext context);
        float GetWeight(EnemyContext context);
    }
}
