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
    public sealed class TagRelayFadeAction : SequencerAction
    {
        public enum FadeTarget
        {
            Invisible,
            OriginalOpacity
        }

        [FoldoutGroup("Target")]
        [SerializeField]
        private PlayerTagRelayVfx.RelayActor actor;

        [FoldoutGroup("Fade")]
        [SerializeField]
        private FadeTarget fadeTarget;

        [FoldoutGroup("Timing")]
        [MinValue(0.0f)]
        [SerializeField]
        private float duration = 0.2f;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private Ease ease = Ease.InOutSine;

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

            float destination = fadeTarget == FadeTarget.Invisible
                ? 0.0f
                : relay.GetActorOriginalAlpha(actor);

            Tween tween = DOTween.To(
                    () => relay.GetActorAlpha(actor),
                    alpha => relay.SetActorAlpha(actor, alpha),
                    destination,
                    Mathf.Max(0.0f, ScaleDuration(duration, context)))
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
