using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Tutorials
{
    public sealed class TutorialRunner : SerializedMonoBehaviour
    {
        [TitleGroup("References")]
        [OdinSerialize, Required]
        private ITutorialPresenter _presenter;

        private CancellationTokenSource _activeCancellation;
        private bool _isRunning;

        private void OnDisable()
        {
            _activeCancellation?.Cancel();

            if (_presenter != null)
                _presenter.HideObjectives();
        }

        public async UniTask RunAsync(TutorialData data, CancellationToken cancellationToken)
        {
            Validate(data);

            if (_isRunning)
                throw new InvalidOperationException("TutorialRunner can only run one TutorialData at a time.");

            CancellationTokenSource linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _activeCancellation = linkedCancellation;
            _isRunning = true;

            List<TutorialObjectiveRuntime> objectives = new List<TutorialObjectiveRuntime>(data.Objectives.Count);

            try
            {
                _presenter.ShowObjectives(data.Objectives);

                foreach (TutorialObjectiveData objective in data.Objectives)
                {
                    if (objective != null && objective.SignalId != TutorialSignalId.None)
                        objectives.Add(new TutorialObjectiveRuntime(objective, _presenter));
                }

                await UniTask.WaitUntil(() => AreCompletionObjectivesComplete(objectives), cancellationToken: linkedCancellation.Token);
            }
            finally
            {

                _presenter.HideObjectives();

                if (ReferenceEquals(_activeCancellation, linkedCancellation))
                    _activeCancellation = null;

                _isRunning = false;
                linkedCancellation.Dispose();
            }
        }

        private static bool AreCompletionObjectivesComplete(IReadOnlyList<TutorialObjectiveRuntime> objectives)
        {

            foreach (TutorialObjectiveRuntime objective in objectives)
            {
                if (!objective.IsCompleted)
                    return false;
            }

            return true;
        }

        private void Validate(TutorialData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (_presenter == null)
                throw new InvalidOperationException("TutorialRunner requires a TutorialPresenter.");

            if (data.Objectives == null || data.Objectives.Count == 0)
                throw new InvalidOperationException($"Tutorial '{data.EditorLabel}' contains no objective.");

            HashSet<string> objectiveIds = new HashSet<string>();
            bool hasCompletableObjective = false;

            foreach (TutorialObjectiveData objective in data.Objectives)
            {
                if (objective == null)
                    throw new InvalidOperationException($"Tutorial '{data.EditorLabel}' contains a null objective.");

                if (string.IsNullOrWhiteSpace(objective.Id))
                    throw new InvalidOperationException($"Tutorial '{data.EditorLabel}' contains an objective without an ID.");

                if (!objectiveIds.Add(objective.Id))
                    throw new InvalidOperationException($"Tutorial '{data.EditorLabel}' contains duplicate objective ID '{objective.Id}'.");

                if (objective.SignalId == TutorialSignalId.None)
                {
                        throw new InvalidOperationException($"Objective '{objective.Id}' has no signal.");
                }

                hasCompletableObjective = true;
            }

            if (!hasCompletableObjective)
            {
                throw new InvalidOperationException($"Tutorial '{data.EditorLabel}' contains no objective connected to a signal.");
            }
        }
    }
}
