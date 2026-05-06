using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    [Serializable]
    public sealed class SetHitboxBehaviourAction : EnemyBehaviourAction
    {
        [FoldoutGroup("Hitbox")]
        [SerializeField]
        private bool activate = true;

        public override UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            if (activate)
                context.Enemy.ActivateHitbox();
            else
                context.Enemy.DeactivateHitbox();

            return UniTask.CompletedTask;
        }
    }
}
