using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    [Serializable]
    public abstract class EnemyBehaviourAction
    {
        [FoldoutGroup("Meta")]
        [SerializeField]
        private bool enabled = true;

        [FoldoutGroup("Meta")]
        [SerializeField]
        private string label = string.Empty;

        [ShowInInspector, ReadOnly, PropertyOrder(-10)]
        public string ListLabel => string.IsNullOrWhiteSpace(label)
            ? GetType().Name
            : label;

        public bool Enabled => enabled;

        public virtual UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public virtual void Cancel(BehaviourContext context)
        {
        }

        public virtual void ResetRuntimeState()
        {
        }

        public override string ToString()
        {
            return ListLabel;
        }
    }
}
