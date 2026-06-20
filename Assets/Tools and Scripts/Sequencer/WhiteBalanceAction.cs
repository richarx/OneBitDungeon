using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TataSequencing
{
    [Serializable]
    public sealed class WhiteBalanceAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Volume volume;

        [FoldoutGroup("White Balance")]
        [SerializeField]
        private bool activateComponent = true;

        [FoldoutGroup("White Balance")]
        [SerializeField]
        private bool forceOverrideState = true;

        [FoldoutGroup("White Balance")]
        [SerializeField]
        private float temperature = 0f;

        [FoldoutGroup("Timing")]
        [MinValue(0f)]
        [SerializeField]
        private float duration = 1f;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private Ease ease = Ease.Linear;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private bool waitAction = true;

        [NonSerialized]
        private Tween activeTween;

        public override async UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            if (!TryGetWhiteBalance(context, out WhiteBalance whiteBalance))
            {
                return;
            }

            Tween tween = BuildTween(context, whiteBalance);
            if (tween == null)
            {
                return;
            }

            activeTween = tween;
            _ = tween.OnKill(() =>
            {
                if (activeTween == tween)
                {
                    activeTween = null;
                }
            });

            _ = tween.Play();

            if (!waitAction)
            {
                return;
            }

            await tween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
        }

        public override void Cancel(SequencerContext context)
        {
            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill(false);
            }

            activeTween = null;
        }

        private Tween BuildTween(SequencerContext context, WhiteBalance whiteBalance)
        {
            float scaledDuration = Mathf.Max(0f, ScaleDuration(duration, context));

            if (activateComponent)
            {
                whiteBalance.active = true;
            }

            if (forceOverrideState)
            {
                whiteBalance.temperature.overrideState = true;
            }

            if (scaledDuration <= 0f)
            {
                whiteBalance.temperature.value = temperature;
                return null;
            }

            return DOTween.To(
                    () => whiteBalance.temperature.value,
                    value => whiteBalance.temperature.value = value,
                    temperature,
                    scaledDuration)
                .SetEase(ease)
                .Pause();
        }

        private bool TryGetWhiteBalance(SequencerContext context, out WhiteBalance whiteBalance)
        {
            whiteBalance = null;

            if (volume == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] WhiteBalanceAction has no Volume target.", context.Owner);
                return false;
            }

            if (volume.profile == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] WhiteBalanceAction requires a Volume with a valid profile.", context.Owner);
                return false;
            }

            if (!volume.profile.TryGet(out whiteBalance) || whiteBalance == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] WhiteBalanceAction requires a White Balance override on the target Volume profile.", context.Owner);
                return false;
            }

            return true;
        }
    }
}
