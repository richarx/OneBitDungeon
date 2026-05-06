using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    [Serializable]
    public sealed class DelayBehaviourAction : EnemyBehaviourAction
    {
        [FoldoutGroup("Timing")]
        [MinValue(0f)]
        [SerializeField]
        private float duration = 1f;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private bool ignoreTimeScale;

        public override async UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            int delayMs = Mathf.Max(0, Mathf.RoundToInt(duration * 1000f));
            DelayType delayType = ignoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime;
            await UniTask.Delay(delayMs, delayType, cancellationToken: cancellationToken);
        }
    }
}
