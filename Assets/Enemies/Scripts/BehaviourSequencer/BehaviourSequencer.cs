using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Enemies.Scripts.Behaviours;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.BehaviourSequencer
{
    public sealed class BehaviourSequencer : MonoBehaviour, IEnemyBehaviour
    {
        [SerializeReference]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(
            DraggableItems = true,
            ShowFoldout = true,
            HideAddButton = false,
            ShowIndexLabels = true,
            NumberOfItemsPerPage = 20,
            ListElementLabelName = nameof(EnemyBehaviourAction.ListLabel))]
        [TypeFilter(nameof(GetActionTypes))]
        private List<EnemyBehaviourAction> actions = new List<EnemyBehaviourAction>();

        [NonSerialized]
        private CancellationTokenSource cts;

        [NonSerialized]
        private BehaviourContext currentContext;

        public void StartBehaviour(EnemyController enemy)
        {
            cts?.Dispose();
            cts = new CancellationTokenSource();
            currentContext = new BehaviourContext(enemy);

            foreach (EnemyBehaviourAction action in actions.Where(a => a != null))
                action.ResetRuntimeState();

            RunAsync(currentContext, cts.Token).Forget();
        }

        private async UniTaskVoid RunAsync(BehaviourContext context, CancellationToken cancellationToken)
        {
            try
            {
                foreach (EnemyBehaviourAction action in actions)
                {
                    if (action == null || !action.Enabled)
                        continue;

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await action.ExecuteAsync(context, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        public void CancelBehaviour(EnemyController enemy)
        {
            if (cts == null)
                return;

            cts.Cancel();

            foreach (EnemyBehaviourAction action in actions.Where(a => a != null))
            {
                try
                {
                    action.Cancel(currentContext);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }

            cts.Dispose();
            cts = null;
        }

        public void StopBehaviour(EnemyController enemy)
        {
        }

        public void UpdateBehaviour(EnemyController enemy)
        {
        }

        public void FixedUpdateBehaviour(EnemyController enemy)
        {
        }

        public void SetSubBehaviourState(bool state)
        {
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
