using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Player.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public sealed class TagRelayMoveAction : SequencerAction
    {
        public enum MoveMode
        {
            BackwardByDistance,
            FromBehindToOrigin
        }

        [FoldoutGroup("Target")]
        [SerializeField]
        private PlayerTagRelayVfx.RelayActor actor;

        [FoldoutGroup("Move")]
        [SerializeField]
        private MoveMode moveMode;

        [FoldoutGroup("Move")]
        [MinValue(0.0f)]
        [SerializeField]
        private float distance = 0.3f;

        [FoldoutGroup("Timing")]
        [MinValue(0.0f)]
        [SerializeField]
        private float duration = 0.2f;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private Ease ease = Ease.OutCubic;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private bool waitAction;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private bool ignoreTimeScale = true;

        [NonSerialized]
        private Tween activeTween;

        public override async UniTask ExecuteAsync(
            SequencerContext context,
            CancellationToken cancellationToken)
        {
            PlayerTagRelayVfx relay = context.GameObject.GetComponent<PlayerTagRelayVfx>();
            if (relay == null || !relay.HasPreparedTransition)
                return;

            SpriteRenderer renderer = relay.GetActorRenderer(actor);
            if (renderer == null)
                return;

            Transform target = renderer.transform;
            if (target == null)
                return;

            Vector3 origin = relay.GetActorOriginalLocalPosition(actor);
            Vector3 offset = relay.GetBackwardLocalOffset(target.parent, distance);
            Vector3 destination;

            if (moveMode == MoveMode.FromBehindToOrigin)
            {
                target.localPosition = origin + offset;
                destination = origin;
            }
            else
            {
                destination = target.localPosition + offset;
            }

            Tween tween = target
                .DOLocalMove(destination, Mathf.Max(0.0f, ScaleDuration(duration, context)))
                .SetEase(ease)
                .SetUpdate(ignoreTimeScale)
                .SetLink(renderer.gameObject, LinkBehaviour.KillOnDestroy);

            activeTween = tween;
            relay.TrackTween(tween);
            _ = tween.OnKill(() =>
            {
                if (activeTween == tween)
                    activeTween = null;
            });

            if (!waitAction)
                return;

            await tween.ToUniTask(
                TweenCancelBehaviour.KillAndCancelAwait,
                cancellationToken);
        }

        public override void Cancel(SequencerContext context)
        {
            if (activeTween != null && activeTween.IsActive())
                activeTween.Kill(false);

            activeTween = null;
        }
    }
}
