using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    [Serializable]
    public sealed class SelectNextBehaviourAction : EnemyBehaviourAction
    {
        [FoldoutGroup("Transition")]
        [SerializeField]
        private bool isFromTransition;

        public override UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            context.Enemy.SelectNewBehaviour(isFromTransition);
            return UniTask.CompletedTask;
        }
    }
}
