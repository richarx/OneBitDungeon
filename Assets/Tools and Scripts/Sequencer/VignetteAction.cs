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
    public sealed class VignetteAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Volume volume;

        [FoldoutGroup("Vignette")]
        [SerializeField]
        private bool activateComponent = true;

        [FoldoutGroup("Vignette")]
        [SerializeField]
        private bool forceOverrideState = true;

        [FoldoutGroup("Vignette")]
        [Range(0f, 1f)]
        [SerializeField]
        private float intensity = 0f;

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
            if (!TryGetVignette(context, out Vignette vignette))
            {
                return;
            }

            Tween tween = BuildTween(context, vignette);
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

        private Tween BuildTween(SequencerContext context, Vignette vignette)
        {
            float scaledDuration = Mathf.Max(0f, ScaleDuration(duration, context));

            if (activateComponent)
            {
                vignette.active = true;
            }

            if (forceOverrideState)
            {
                vignette.intensity.overrideState = true;
            }

            float clampedIntensity = Mathf.Clamp01(intensity);

            if (scaledDuration <= 0f)
            {
                vignette.intensity.value = clampedIntensity;
                return null;
            }

            return DOTween.To(
                    () => vignette.intensity.value,
                    value => vignette.intensity.value = value,
                    clampedIntensity,
                    scaledDuration)
                .SetEase(ease)
                .Pause();
        }

        private bool TryGetVignette(SequencerContext context, out Vignette vignette)
        {
            vignette = null;

            if (volume == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] VignetteAction has no Volume target.", context.Owner);
                return false;
            }

            if (volume.profile == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] VignetteAction requires a Volume with a valid profile.", context.Owner);
                return false;
            }

            if (!volume.profile.TryGet(out vignette) || vignette == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] VignetteAction requires a Vignette override on the target Volume profile.", context.Owner);
                return false;
            }

            return true;
        }
    }
}
