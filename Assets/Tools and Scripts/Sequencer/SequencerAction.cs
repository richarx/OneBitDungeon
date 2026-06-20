using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public abstract class SequencerAction
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

        public string Label => string.IsNullOrWhiteSpace(label)
            ? GetType().Name
            : label;

        public virtual bool IsControlFlow => false;

        public virtual UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        public virtual void ResetRuntimeState()
        {
        }

        public virtual void Cancel(SequencerContext context)
        {
        }

        protected float ScaleDuration(float duration, SequencerContext context)
        {
            return duration / Mathf.Max(0.01f, context.SpeedFactor);
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
