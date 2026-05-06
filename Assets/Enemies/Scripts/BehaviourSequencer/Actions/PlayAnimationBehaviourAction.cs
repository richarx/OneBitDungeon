using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    [Serializable]
    public sealed class PlayAnimationBehaviourAction : EnemyBehaviourAction
    {
        [FoldoutGroup("Animation")]
        [SerializeField]
        private string clipName;

        public override UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(clipName))
                context.Enemy.animator.Play(clipName);

            return UniTask.CompletedTask;
        }
    }
}
