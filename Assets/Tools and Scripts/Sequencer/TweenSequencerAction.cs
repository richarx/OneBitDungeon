using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace TataSequencing
{
    [Serializable]
    public abstract class TweenSequencerAction : SequencerAction
    {
        [NonSerialized]
        private Tween activeTween;

        protected UniTask PlayTweenAsync(Tween tween, CancellationToken cancellationToken)
        {
            if (tween == null || !tween.IsActive())
            {
                return UniTask.CompletedTask;
            }

            activeTween = tween;
            return tween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
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
