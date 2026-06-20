using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public sealed class AudioVolumeAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private AudioSource targetAudioSource;

        [FoldoutGroup("Audio")]
        [Range(0f, 1f)]
        [SerializeField]
        private float volume = 1f;

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
            if (targetAudioSource == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] AudioVolumeAction has no AudioSource target.", context.Owner);
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
            float scaledDuration = Mathf.Max(0f, ScaleDuration(duration, context));
            float clampedVolume = Mathf.Clamp01(volume);

            if (scaledDuration <= 0f)
            {
                targetAudioSource.volume = clampedVolume;
                return null;
            }

            return DOTween.To(
                    () => targetAudioSource.volume,
                    value => targetAudioSource.volume = value,
                    clampedVolume,
                    scaledDuration)
                .SetEase(ease)
                .Pause();
        }
    }
}
