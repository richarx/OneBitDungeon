using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    public enum PauseMode
    {
        Constant,
        Random
    }

    [Serializable]
    public class PauseAction : SequencerAction
    {
        [SerializeField]
        private PauseMode pauseMode = PauseMode.Constant;

        [ShowIf(nameof(IsConstantMode))]
        [MinValue(0f)]
        [SerializeField]
        private float duration = 1f;

        [ShowIf(nameof(IsRandomMode))]
        [MinValue(0f)]
        [SerializeField]
        private float durationMin = 1f;

        [ShowIf(nameof(IsRandomMode))]
        [MinValue(0f)]
        [SerializeField]
        private float durationMax = 2f;

        [NonSerialized]
        private CancellationTokenSource localCancellationToken;

        private bool IsConstantMode => pauseMode == PauseMode.Constant;
        private bool IsRandomMode => pauseMode == PauseMode.Random;

        public override async UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            localCancellationToken?.Dispose();
            localCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                var seconds = GetDurationSeconds();
                await UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: localCancellationToken.Token);
            }
            finally
            {
                localCancellationToken?.Dispose();
                localCancellationToken = null;
            }
        }

        public override void Cancel(SequencerContext context)
        {
            localCancellationToken?.Cancel();
        }

        private float GetDurationSeconds()
        {
            if (pauseMode == PauseMode.Constant)
            {
                return Mathf.Max(0f, duration);
            }

            var min = Mathf.Min(durationMin, durationMax);
            var max = Mathf.Max(durationMin, durationMax);
            return UnityEngine.Random.Range(min, max);
        }
    }
}
