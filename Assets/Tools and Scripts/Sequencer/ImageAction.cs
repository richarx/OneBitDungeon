using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace TataSequencing
{
    [Serializable]
    public sealed class ImageAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Image targetImage;

        [FoldoutGroup("Image")]
        [Range(0f, 1f)]
        [SerializeField]
        private float opacity = 1f;

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
            if (targetImage == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] ImageAction has no Image target.", context.Owner);
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
            float clampedOpacity = Mathf.Clamp01(opacity);

            if (scaledDuration <= 0f)
            {
                SetAlpha(clampedOpacity);
                return null;
            }

            return DOTween.To(
                    () => targetImage.color.a,
                    SetAlpha,
                    clampedOpacity,
                    scaledDuration)
                .SetEase(ease)
                .Pause();
        }

        private void SetAlpha(float alpha)
        {
            Color color = targetImage.color;
            color.a = alpha;
            targetImage.color = color;
        }
    }
}
