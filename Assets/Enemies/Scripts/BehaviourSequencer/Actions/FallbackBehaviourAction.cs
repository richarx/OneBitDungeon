using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Enemies.Scripts.Behaviours;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    [Serializable]
    public sealed class FallbackBehaviourAction : EnemyBehaviourAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private MonoBehaviour target;

        public override UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                Debug.LogWarning($"[FallbackBehaviourAction] Target is null.");
                return UniTask.CompletedTask;
            }

            IEnemyBehaviour behaviour = target.GetComponent<IEnemyBehaviour>();

            if (behaviour == null)
            {
                Debug.LogWarning($"[FallbackBehaviourAction] Target '{target.name}' has no IEnemyBehaviour component.");
                return UniTask.CompletedTask;
            }

            behaviour.StartBehaviour(context.Enemy);
            return UniTask.CompletedTask;
        }
    }
}
