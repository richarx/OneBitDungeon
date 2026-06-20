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
    public sealed class FilmGrainAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Volume targetVolume;

        [FoldoutGroup("Grain")]
        [SerializeField]
        private bool changeType = true;

        [FoldoutGroup("Grain")]
        [ShowIf(nameof(changeType))]
        [SerializeField]
        private FilmGrainLookup grainType = FilmGrainLookup.Medium1;

        [FoldoutGroup("Grain")]
        [ShowIf("@changeType && grainType == FilmGrainLookup.Custom")]
        [SerializeField]
        private Texture customTexture;

        [FoldoutGroup("Grain")]
        [SerializeField]
        private bool changeIntensity = true;

        [FoldoutGroup("Grain")]
        [ShowIf(nameof(changeIntensity))]
        [MinValue(0f)]
        [SerializeField]
        private float targetIntensity = 0.5f;

        [FoldoutGroup("Grain")]
        [ShowIf(nameof(changeIntensity))]
        [MinValue(0f)]
        [SerializeField]
        private float duration = 0.35f;

        [FoldoutGroup("Grain")]
        [ShowIf(nameof(changeIntensity))]
        [SerializeField]
        private Ease ease = Ease.Linear;

        [FoldoutGroup("Grain")]
        [SerializeField]
        private bool setTypeBeforeTween = true;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private bool waitAction = true;

        [NonSerialized]
        private Tween activeTween;

        public override async UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            if (targetVolume == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] FilmGrainAction has no target volume.", context.Owner);
                return;
            }

            VolumeProfile profile = targetVolume.profile;
            if (profile == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] FilmGrainAction target volume has no profile.", targetVolume);
                return;
            }

            if (!profile.TryGet(out FilmGrain filmGrain) || filmGrain == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] FilmGrainAction requires a Film Grain override on the target volume profile.", targetVolume);
                return;
            }

            if (changeType && setTypeBeforeTween)
            {
                ApplyType(filmGrain);
            }

            Tween tween = BuildTween(filmGrain, context);
            if (tween == null)
            {
                if (changeType && !setTypeBeforeTween)
                {
                    ApplyType(filmGrain);
                }

                return;
            }

            if (changeType && !setTypeBeforeTween)
            {
                _ = tween.OnComplete(() => ApplyType(filmGrain));
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

        private Tween BuildTween(FilmGrain filmGrain, SequencerContext context)
        {
            if (!changeIntensity)
            {
                return null;
            }

            float scaledDuration = Mathf.Max(0f, ScaleDuration(duration, context));
            if (scaledDuration <= 0f)
            {
                filmGrain.intensity.overrideState = true;
                filmGrain.intensity.value = targetIntensity;
                return null;
            }

            filmGrain.intensity.overrideState = true;

            return DOTween
                .To(() => filmGrain.intensity.value,
                    value => filmGrain.intensity.value = value,
                    targetIntensity,
                    scaledDuration)
                .SetEase(ease)
                .Pause();
        }

        private void ApplyType(FilmGrain filmGrain)
        {
            if (!changeType)
            {
                return;
            }

            filmGrain.type.overrideState = true;
            filmGrain.type.value = grainType;

            if (grainType == FilmGrainLookup.Custom)
            {
                if (customTexture == null)
                {
                    Debug.LogWarning("FilmGrainAction is set to Custom but no custom texture is assigned.");
                }

                filmGrain.texture.overrideState = true;
                filmGrain.texture.value = customTexture;
            }
        }
    }
}
