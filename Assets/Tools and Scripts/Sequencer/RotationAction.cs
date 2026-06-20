using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public sealed class RotationAction : SequencerAction
    {
        public enum RotationMode
        {
            By,
            To
        }

        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Transform target;

        [FoldoutGroup("Rotation")]
        [SerializeField]
        private ActionSpace rotationSpace = ActionSpace.Local;

        [FoldoutGroup("Rotation")]
        [SerializeField]
        private RotationMode rotationMode = RotationMode.By;

        [FoldoutGroup("Rotation")]
        [ShowIf("@rotationMode == RotationMode.By")]
        [SerializeField]
        private Vector3 rotateBy = new(100f, 0f, 0f);

        [FoldoutGroup("Rotation")]
        [ShowIf("@rotationMode == RotationMode.To")]
        [SerializeField]
        private Vector3 rotateTo = new(100f, 0f, 0f);

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
            if (target == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] RotationAction has no target.", context.Owner);
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

        private Tween BuildTween(SequencerContext context)
        {
            float calculatedDuration = Mathf.Max(0f, ScaleDuration(duration, context));

            switch (rotationMode)
            {
                case RotationMode.By:
                    return rotationSpace == ActionSpace.Local
                        ? target.DOLocalRotate(rotateBy, calculatedDuration, RotateMode.FastBeyond360)
                            .SetRelative()
                            .SetEase(ease)
                        : target.DORotate(rotateBy, calculatedDuration, RotateMode.FastBeyond360)
                            .SetRelative()
                            .SetEase(ease);

                case RotationMode.To:
                    return rotationSpace == ActionSpace.Local
                        ? target.DOLocalRotate(rotateTo, calculatedDuration, RotateMode.Fast)
                            .SetEase(ease)
                        : target.DORotate(rotateTo, calculatedDuration, RotateMode.Fast)
                            .SetEase(ease);

                default:
                    return null;
            }
        }
    }
}
