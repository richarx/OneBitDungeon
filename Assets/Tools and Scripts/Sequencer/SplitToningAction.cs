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
    public sealed class SplitToningAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Volume volume;

        [FoldoutGroup("Split Toning")]
        [SerializeField]
        private bool activateComponent = true;

        [FoldoutGroup("Split Toning")]
        [SerializeField]
        private bool forceOverrideState = true;

        [FoldoutGroup("Split Toning")]
        [SerializeField]
        private Color shadows = Color.gray;

        [FoldoutGroup("Split Toning")]
        [SerializeField]
        private Color highlights = Color.gray;

        [FoldoutGroup("Split Toning")]
        [SerializeField]
        private float balance = 0f;

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
            if (!TryGetSplitToning(context, out SplitToning splitToning))
            {
                return;
            }

            Tween tween = BuildTween(context, splitToning);
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

        private Tween BuildTween(SequencerContext context, SplitToning splitToning)
        {
            float scaledDuration = Mathf.Max(0f, ScaleDuration(duration, context));

            if (activateComponent)
            {
                splitToning.active = true;
            }

            if (forceOverrideState)
            {
                splitToning.shadows.overrideState = true;
                splitToning.highlights.overrideState = true;
                splitToning.balance.overrideState = true;
            }

            if (scaledDuration <= 0f)
            {
                splitToning.shadows.value = shadows;
                splitToning.highlights.value = highlights;
                splitToning.balance.value = balance;
                return null;
            }

            Sequence sequence = DOTween.Sequence();
            _ = sequence.Join(DOTween.To(
                () => splitToning.shadows.value,
                value => splitToning.shadows.value = value,
                shadows,
                scaledDuration));

            _ = sequence.Join(DOTween.To(
                () => splitToning.highlights.value,
                value => splitToning.highlights.value = value,
                highlights,
                scaledDuration));

            _ = sequence.Join(DOTween.To(
                () => splitToning.balance.value,
                value => splitToning.balance.value = value,
                balance,
                scaledDuration));

            return sequence.SetEase(ease).Pause();
        }

        private bool TryGetSplitToning(SequencerContext context, out SplitToning splitToning)
        {
            splitToning = null;

            if (volume == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] SplitToningAction has no Volume target.", context.Owner);
                return false;
            }

            if (volume.profile == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] SplitToningAction requires a Volume with a valid profile.", context.Owner);
                return false;
            }

            if (!volume.profile.TryGet(out splitToning) || splitToning == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] SplitToningAction requires a Split Toning override on the target Volume profile.", context.Owner);
                return false;
            }

            return true;
        }
    }
}
