using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace TataSequencing
{
    [Serializable]
    public class EventAction : SequencerAction
    {
        [FoldoutGroup("Events")]
        [SerializeField]
        private UnityEvent onExecute = new();

        public override UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            onExecute?.Invoke();
            return UniTask.CompletedTask;
        }
    }
}
