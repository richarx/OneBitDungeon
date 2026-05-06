using System.Collections.Generic;

namespace Enemies.BehaviourSequencer
{
    public sealed class BehaviourContext
    {
        public BehaviourContext(EnemyController enemy)
        {
            Enemy = enemy;
        }

        public EnemyController Enemy { get; }

        public List<SpinRock> Rocks { get; } = new List<SpinRock>();

        public bool IsSecondPhase => Enemy.currentPhase > 0;
    }
}
