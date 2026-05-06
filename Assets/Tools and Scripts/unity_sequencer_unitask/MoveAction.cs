using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{


    [Serializable]
    public sealed class MoveAction : SequencerAction
    {
        public enum MoveMode
        {
            By,
            BetweenAnchors,
            To
        }

        public enum DurationMode
        {
            Fixed,
            BasedOnSpeed
        }

        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private Transform target;

        [FoldoutGroup("Move")]
        [SerializeField]
        private MoveMode moveMode = MoveMode.By;

        [FoldoutGroup("Move")]
        [SerializeField]
        private ActionSpace actionSpace = ActionSpace.World;

        [FoldoutGroup("Move")]
        [SerializeField]
        private DurationMode durationMode = DurationMode.Fixed;

        [FoldoutGroup("Move")]
        [ShowIf("@moveMode == MoveMode.By")]
        [SerializeField]
        private Vector3 moveBy = new(1f, 0f, 0f);

        [FoldoutGroup("Move")]
        [ShowIf("@moveMode == MoveMode.To && actionSpace == ActionSpace.World")]
        [SerializeField]
        private Vector3 worldDestination = Vector3.zero;

        [FoldoutGroup("Move")]
        [ShowIf("@moveMode == MoveMode.To && actionSpace == ActionSpace.Local")]
        [SerializeField]
        private Vector3 localDestination = Vector3.zero;

        [FoldoutGroup("Move")]
        [ShowIf("@moveMode == MoveMode.BetweenAnchors")]
        [Required]
        [SerializeField]
        private Transform anchorStart;

        [FoldoutGroup("Move")]
        [ShowIf("@moveMode == MoveMode.BetweenAnchors")]
        [Required]
        [SerializeField]
        private Transform anchorEnd;

        [FoldoutGroup("Timing")]
        [ShowIf("@durationMode == DurationMode.Fixed")]
        [MinValue(0f)]
        [SerializeField]
        private float duration = 1f;

        [FoldoutGroup("Timing")]
        [ShowIf("@durationMode == DurationMode.BasedOnSpeed")]
        [MinValue(0.0001f)]
        [SerializeField]
        private float speed = 1f;

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
                Debug.LogWarning($"[{context.Owner.name}] MoveAction has no target.", context.Owner);
                return;
            }

            if (moveMode == MoveMode.BetweenAnchors && (anchorStart == null || anchorEnd == null))
            {
                Debug.LogWarning($"[{context.Owner.name}] MoveAction in BetweenAnchors mode requires both anchors.", context.Owner);
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
            float calculatedDuration = CalculateDuration(context);

            switch (moveMode)
            {
                case MoveMode.By:
                    return actionSpace == ActionSpace.Local
                        ? target.DOLocalMove(target.localPosition + moveBy, calculatedDuration).SetEase(ease)
                        : target.DOMove(target.position + moveBy, calculatedDuration).SetEase(ease);

                case MoveMode.To:
                    return actionSpace == ActionSpace.Local
                        ? target.DOLocalMove(localDestination, calculatedDuration).SetEase(ease)
                        : target.DOMove(worldDestination, calculatedDuration).SetEase(ease);

                case MoveMode.BetweenAnchors:
                    target.position = anchorStart.position;
                    return target.DOMove(anchorEnd.position, calculatedDuration).SetEase(ease);

                default:
                    return null;
            }
        }

        private float CalculateDuration(SequencerContext context)
        {
            float calculatedDuration = duration;
            if (durationMode == DurationMode.BasedOnSpeed)
            {
                float distance = CalculateDistance();
                calculatedDuration = distance / Mathf.Max(0.0001f, speed);
            }

            return Mathf.Max(0f, ScaleDuration(calculatedDuration, context));
        }

        private float CalculateDistance()
        {
            switch (moveMode)
            {
                case MoveMode.By:
                    return moveBy.magnitude;

                case MoveMode.To:
                    return actionSpace == ActionSpace.Local
                        ? Vector3.Distance(target.localPosition, localDestination)
                        : Vector3.Distance(target.position, worldDestination);

                case MoveMode.BetweenAnchors:
                    if (anchorStart == null || anchorEnd == null)
                    {
                        return 0f;
                    }

                    return Vector3.Distance(anchorStart.position, anchorEnd.position);

                default:
                    return 0f;
            }
        }
    }
}
