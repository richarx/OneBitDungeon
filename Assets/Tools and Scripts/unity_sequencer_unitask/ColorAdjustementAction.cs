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
    public sealed class ColorAdjustementAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Volume volume;

        [FoldoutGroup("Color Adjustments")]
        [SerializeField]
        private bool activateComponent = true;

        [FoldoutGroup("Color Adjustments")]
        [SerializeField]
        private bool forceOverrideState = true;

        [FoldoutGroup("Color Adjustments")]
        [SerializeField]
        private float contrast = 0f;

        [FoldoutGroup("Color Adjustments")]
        [SerializeField]
        private float saturation = 0f;

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
            if (!TryGetColorAdjustments(context, out ColorAdjustments colorAdjustments))
            {
                return;
            }

            Tween tween = BuildTween(context, colorAdjustments);
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

        private Tween BuildTween(SequencerContext context, ColorAdjustments colorAdjustments)
        {
            float scaledDuration = Mathf.Max(0f, ScaleDuration(duration, context));

            if (activateComponent)
            {
                colorAdjustments.active = true;
            }

            if (forceOverrideState)
            {
                colorAdjustments.contrast.overrideState = true;
                colorAdjustments.saturation.overrideState = true;
            }

            if (scaledDuration <= 0f)
            {
                colorAdjustments.contrast.value = contrast;
                colorAdjustments.saturation.value = saturation;
                return null;
            }

            Sequence sequence = DOTween.Sequence();
            _ = sequence.Join(DOTween.To(
                () => colorAdjustments.contrast.value,
                value => colorAdjustments.contrast.value = value,
                contrast,
                scaledDuration));

            _ = sequence.Join(DOTween.To(
                () => colorAdjustments.saturation.value,
                value => colorAdjustments.saturation.value = value,
                saturation,
                scaledDuration));

            return sequence.SetEase(ease).Pause();
        }

        private bool TryGetColorAdjustments(SequencerContext context, out ColorAdjustments colorAdjustments)
        {
            colorAdjustments = null;

            if (volume == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] ColorAdjustementAction has no Volume target.", context.Owner);
                return false;
            }

            if (volume.profile == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] ColorAdjustementAction requires a Volume with a valid profile.", context.Owner);
                return false;
            }

            if (!volume.profile.TryGet(out colorAdjustments) || colorAdjustments == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] ColorAdjustementAction requires a Color Adjustments override on the target Volume profile.", context.Owner);
                return false;
            }

            return true;
        }
    }
}
