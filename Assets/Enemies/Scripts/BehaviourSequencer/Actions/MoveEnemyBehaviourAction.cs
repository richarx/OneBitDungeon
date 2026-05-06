using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Player.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    [Serializable]
    public sealed class MoveEnemyBehaviourAction : EnemyBehaviourAction
    {
        public enum TargetMode
        {
            Fixed,
            RandomInRadius,
        }

        [FoldoutGroup("Move")]
        [SerializeField]
        private TargetMode targetMode = TargetMode.RandomInRadius;

        [FoldoutGroup("Move")]
        [ShowIf("@targetMode == TargetMode.Fixed")]
        [SerializeField]
        private Vector3 fixedPosition;

        [FoldoutGroup("Move")]
        [ShowIf("@targetMode == TargetMode.RandomInRadius")]
        [MinValue(0f)]
        [SerializeField]
        private float radius = 7f;

        [FoldoutGroup("Move")]
        [MinValue(0f)]
        [SerializeField]
        private float duration = 1f;

        [FoldoutGroup("Move")]
        [SerializeField]
        private Ease ease = Ease.InOutCubic;

        [FoldoutGroup("Move")]
        [SerializeField]
        private bool triggerAfterImage;

        [NonSerialized]
        private Tween activeTween;

        public override async UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            Vector3 target = ComputeTarget();

            float moveDuration = context.IsSecondPhase ? duration * 0.5f : duration;

            if (triggerAfterImage && context.IsSecondPhase)
                context.Enemy.afterImage.Trigger(moveDuration);

            activeTween = context.Enemy.transform.DOMove(target, moveDuration).SetEase(ease);

            await activeTween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
            activeTween = null;
        }

        public override void Cancel(BehaviourContext context)
        {
            if (activeTween != null && activeTween.IsActive())
                activeTween.Kill();

            activeTween = null;
        }

        private Vector3 ComputeTarget()
        {
            if (targetMode == TargetMode.Fixed)
                return fixedPosition;

            Vector3 random = UnityEngine.Random.insideUnitSphere * radius;
            random.y = 0f;
            return random;
        }
    }
}
