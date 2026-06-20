using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public sealed class ShakeAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Transform target;

        [FoldoutGroup("Shake")]
        [SerializeField]
        private bool shakePosition = true;

        [FoldoutGroup("Shake")]
        [ShowIf(nameof(shakePosition))]
        [MinValue(0f)]
        [SerializeField]
        private float positionStrength = 0.25f;

        [FoldoutGroup("Shake")]
        [ShowIf(nameof(shakePosition))]
        [MinValue(1)]
        [SerializeField]
        private int positionVibrato = 10;

        [FoldoutGroup("Shake")]
        [ShowIf(nameof(shakePosition))]
        [Range(0f, 180f)]
        [SerializeField]
        private float positionRandomness = 90f;

        [FoldoutGroup("Shake")]
        [ShowIf(nameof(shakePosition))]
        [SerializeField]
        private bool snapping;

        [FoldoutGroup("Shake")]
        [SerializeField]
        private bool shakeRotation = true;

        [FoldoutGroup("Shake")]
        [ShowIf(nameof(shakeRotation))]
        [MinValue(0f)]
        [SerializeField]
        private float rotationStrength = 15f;

        [FoldoutGroup("Shake")]
        [ShowIf(nameof(shakeRotation))]
        [MinValue(1)]
        [SerializeField]
        private int rotationVibrato = 10;

        [FoldoutGroup("Shake")]
        [ShowIf(nameof(shakeRotation))]
        [Range(0f, 180f)]
        [SerializeField]
        private float rotationRandomness = 90f;

        [FoldoutGroup("Timing")]
        [MinValue(0f)]
        [SerializeField]
        private float duration = 0.35f;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private bool waitAction = true;

        [NonSerialized]
        private Tween activeTween;

        public override async UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] ShakeAction has no target.", context.Owner);
                return;
            }

            Tween tween = BuildTween(context);
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

        private Tween BuildTween(SequencerContext context)
        {
            if (!shakePosition && !shakeRotation)
            {
                return null;
            }

            float scaledDuration = Mathf.Max(0f, ScaleDuration(duration, context));
            if (scaledDuration <= 0f)
            {
                return null;
            }

            Sequence sequence = DOTween.Sequence().Pause();

            if (shakePosition)
            {
                sequence.Join(target.DOShakePosition(
                    scaledDuration,
                    positionStrength,
                    positionVibrato,
                    positionRandomness,
                    snapping));
            }

            if (shakeRotation)
            {
                sequence.Join(target.DOShakeRotation(
                    scaledDuration,
                    rotationStrength,
                    rotationVibrato,
                    rotationRandomness));
            }

            return sequence;
        }
    }
}
