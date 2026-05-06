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
    public sealed class TeleportEnemyBehaviourAction : EnemyBehaviourAction
    {
        public enum TargetMode
        {
            Fixed,
            RandomInRadius,
            OppositeOfPlayer,
        }

        [FoldoutGroup("Teleport")]
        [SerializeField]
        private TargetMode targetMode = TargetMode.OppositeOfPlayer;

        [FoldoutGroup("Teleport")]
        [ShowIf("@targetMode == TargetMode.Fixed")]
        [SerializeField]
        private Vector3 fixedPosition;

        [FoldoutGroup("Teleport")]
        [ShowIf("@targetMode == TargetMode.RandomInRadius")]
        [MinValue(0f)]
        [SerializeField]
        private float radius = 7f;

        [FoldoutGroup("Teleport")]
        [ShowIf("@targetMode == TargetMode.OppositeOfPlayer")]
        [MinValue(0f)]
        [SerializeField]
        private float distanceFromCenter = 7f;

        [FoldoutGroup("Teleport")]
        [MinValue(0f)]
        [SerializeField]
        private float scaleDuration = 0.3f;

        [NonSerialized]
        private Tween scaleInTween;

        [NonSerialized]
        private Tween scaleOutTween;

        public override async UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            Transform t = context.Enemy.transform;

            scaleInTween = t.DOScaleX(0f, scaleDuration).SetEase(Ease.InBack);
            await scaleInTween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
            scaleInTween = null;

            t.position = ComputeTarget(context);

            scaleOutTween = t.DOScaleX(1f, scaleDuration).SetEase(Ease.OutBack);
            await scaleOutTween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
            scaleOutTween = null;
        }

        public override void Cancel(BehaviourContext context)
        {
            if (scaleInTween != null && scaleInTween.IsActive())
                scaleInTween.Kill();

            if (scaleOutTween != null && scaleOutTween.IsActive())
                scaleOutTween.Kill();

            scaleInTween = null;
            scaleOutTween = null;
        }

        private Vector3 ComputeTarget(BehaviourContext context)
        {
            switch (targetMode)
            {
                case TargetMode.Fixed:
                    return fixedPosition;

                case TargetMode.RandomInRadius:
                {
                    Vector3 random = UnityEngine.Random.insideUnitSphere * radius;
                    random.y = 0f;
                    return random;
                }

                case TargetMode.OppositeOfPlayer:
                {
                    Vector3 enemyPos = context.Enemy.transform.position;
                    Vector3 playerPos = PlayerStateMachine.instance.position;
                    Vector3 toPlayer = (playerPos - enemyPos).normalized;
                    Vector3 target = playerPos + toPlayer * 0.5f;

                    if (target.magnitude <= 0.01f)
                        return Vector3.forward * distanceFromCenter;

                    return (-target).normalized * distanceFromCenter;
                }

                default:
                    return Vector3.zero;
            }
        }
    }
}
