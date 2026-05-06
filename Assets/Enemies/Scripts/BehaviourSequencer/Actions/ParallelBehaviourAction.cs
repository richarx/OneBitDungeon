using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    [Serializable]
    public sealed class ParallelBehaviourAction : EnemyBehaviourAction
    {
        [SerializeReference]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(
            DraggableItems = true,
            ShowFoldout = true,
            HideAddButton = false,
            ShowIndexLabels = true,
            ListElementLabelName = nameof(EnemyBehaviourAction.ListLabel))]
        [TypeFilter(nameof(GetActionTypes))]
        [FoldoutGroup("Actions")]
        private List<EnemyBehaviourAction> actions = new List<EnemyBehaviourAction>();

        public override async UniTask ExecuteAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            if (actions == null || actions.Count == 0)
                return;

            UniTask[] tasks = actions
                .Where(a => a != null && a.Enabled)
                .Select(a => a.ExecuteAsync(context, cancellationToken))
                .ToArray();

            await UniTask.WhenAll(tasks);
        }

        public override void Cancel(BehaviourContext context)
        {
            if (actions == null)
                return;

            foreach (EnemyBehaviourAction action in actions.Where(a => a != null))
            {
                try
                {
                    action.Cancel(context);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        public override void ResetRuntimeState()
        {
            if (actions == null)
                return;

            foreach (EnemyBehaviourAction action in actions.Where(a => a != null))
                action.ResetRuntimeState();
        }

        private IEnumerable<Type> GetActionTypes()
        {
            return typeof(EnemyBehaviourAction).Assembly
                .GetTypes()
                .Where(type =>
                    typeof(EnemyBehaviourAction).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsGenericType);
        }
    }
}
