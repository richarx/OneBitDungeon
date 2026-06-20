using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public class ScaleAction : SequencerAction
    {
        public enum ScaleMode
        {
            To,
            FromTo,
            From
        }

        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Transform target;

        [FoldoutGroup("Scale")]
        [SerializeField]
        private ScaleMode scaleMode = ScaleMode.To;

        [FoldoutGroup("Scale")]
        [ShowIf(nameof(UsesScaleFrom))]
        [SerializeField]
        private Vector3 scaleFrom = Vector3.zero;

        [FoldoutGroup("Scale")]
        [ShowIf(nameof(UsesScaleTo))]
        [SerializeField]
        private Vector3 scaleTo = Vector3.one;

        [FoldoutGroup("Timing")]
        [MinValue(0f)]
        [SerializeField]
        private float duration = 1f;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private Ease ease = Ease.Linear;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private bool waitAction = false;

        [NonSerialized]
        private Tween activeTween;

        private bool UsesScaleFrom => scaleMode == ScaleMode.FromTo || scaleMode == ScaleMode.From;
        private bool UsesScaleTo => scaleMode == ScaleMode.FromTo || scaleMode == ScaleMode.To;

        public override async UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            if (target == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] ScaleAction has no target.", context.Owner);
                return;
            }

            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill(false);
                activeTween = null;
            }

            Vector3 from = UsesScaleFrom ? scaleFrom : target.localScale;
            Vector3 to = UsesScaleTo ? scaleTo : target.localScale;

            target.localScale = from;

            Tween tween = target
                .DOScale(to, Mathf.Max(0f, duration))
                .SetEase(ease);

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

            await tween.ToUniTask(
                tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait,
                cancellationToken: cancellationToken);
        }

        public override void Cancel(SequencerContext context)
        {
            if (activeTween != null && activeTween.IsActive())
            {
                activeTween.Kill(false);
            }

            activeTween = null;
        }
    }
}
