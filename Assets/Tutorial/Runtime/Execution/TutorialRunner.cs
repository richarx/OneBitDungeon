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

        [TitleGroup("Presentation")]
        [SerializeField, MinValue(0.0f)]
        [Tooltip("Keeps the completed objective visible briefly before the next exercise starts.")]
        private float _completionDisplayDuration = 0.65f;

        private CancellationTokenSource _activeCancellation;
        private readonly List<TutorialObjectiveRuntime> _activeObjectives = new List<TutorialObjectiveRuntime>();
        private bool _isRunning;

        public event Action<TutorialData> OnObjectivesCompleted;

        public bool IsRunning => _isRunning;

        private void OnDisable()
        {
            _activeCancellation?.Cancel();
            DisposeActiveObjectives();

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

            _activeObjectives.Clear();

            try
            {
                _presenter.ShowObjectives(data.Objectives);

                foreach (TutorialObjectiveData objective in data.Objectives)
                {
                    if (objective != null && objective.SignalId != TutorialSignalId.None)
                        _activeObjectives.Add(new TutorialObjectiveRuntime(objective, _presenter));
                }

                await UniTask.WaitUntil(() => AreCompletionObjectivesComplete(_activeObjectives), cancellationToken: linkedCancellation.Token);

                OnObjectivesCompleted?.Invoke(data);

                if (_completionDisplayDuration > 0.0f)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_completionDisplayDuration),
                        cancellationToken: linkedCancellation.Token);
                }
            }
            finally
            {
                DisposeActiveObjectives();
                _presenter.HideObjectives();

                if (ReferenceEquals(_activeCancellation, linkedCancellation))
                    _activeCancellation = null;

                _isRunning = false;
                linkedCancellation.Dispose();
            }
        }

        private void DisposeActiveObjectives()
        {
            foreach (TutorialObjectiveRuntime objective in _activeObjectives)
                objective.Dispose();

            _activeObjectives.Clear();
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
