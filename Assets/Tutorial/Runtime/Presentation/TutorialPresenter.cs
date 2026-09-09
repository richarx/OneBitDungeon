using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tutorials
{
    public sealed class TutorialPresenter : MonoBehaviour, ITutorialPresenter
    {
        [TitleGroup("References")]
        [SerializeField]
        private GameObject _panelRoot;

        [TitleGroup("References")]
        [SerializeField, Required]
        private Transform _objectivesContainer;

        [TitleGroup("References")]
        [SerializeField, Required]
        [Tooltip("Prefab or inactive scene object used as the model for each objective row.")]
        private TutorialObjectiveRow _objectiveRowTemplate;

        private readonly Dictionary<string, TutorialObjectiveViewState> _statesById =
            new Dictionary<string, TutorialObjectiveViewState>();

        private readonly Dictionary<string, TutorialObjectiveRow> _rowsById =
            new Dictionary<string, TutorialObjectiveRow>();

        public void ShowObjectives(IReadOnlyList<TutorialObjectiveData> objectives)
        {
            ClearRows();

            if (_objectivesContainer == null || _objectiveRowTemplate == null)
            {
                Debug.LogError("[Tutorial Presenter] Missing the objectives container or row template.", this);
                return;
            }

            if (_panelRoot != null)
                _panelRoot.SetActive(true);

            if (objectives == null)
                return;

            foreach (TutorialObjectiveData objective in objectives)
            {
                if (!CanDisplay(objective))
                    continue;

                TutorialObjectiveViewState state = new TutorialObjectiveViewState(objective);
                TutorialObjectiveRow row = Instantiate(_objectiveRowTemplate, _objectivesContainer);
                row.gameObject.SetActive(true);

                _statesById.Add(objective.Id, state);
                _rowsById.Add(objective.Id, row);
                RefreshRow(state, row);
            }
        }

        public void SetObjectiveProgress(string objectiveId, int current, int target)
        {
            if (!TryGetObjective(objectiveId, out TutorialObjectiveViewState state, out TutorialObjectiveRow row))
                return;

            state.SetProgress(current, target);
            RefreshRow(state, row);
        }

        public void CompleteObjective(string objectiveId)
        {
            if (!TryGetObjective(objectiveId, out TutorialObjectiveViewState state, out TutorialObjectiveRow row))
                return;

            state.Complete();
            RefreshRow(state, row);
        }

        public void HideObjectives()
        {
            ClearRows();

            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        private bool CanDisplay(TutorialObjectiveData objective)
        {
            if (objective == null)
            {
                Debug.LogWarning("[Tutorial Presenter] A null objective was ignored.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(objective.Id))
            {
                Debug.LogWarning("[Tutorial Presenter] An objective with an empty ID was ignored.", this);
                return false;
            }

            if (_statesById.ContainsKey(objective.Id))
            {
                Debug.LogWarning($"[Tutorial Presenter] Duplicate objective ID '{objective.Id}' was ignored.", this);
                return false;
            }

            return true;
        }

        private bool TryGetObjective(
            string objectiveId,
            out TutorialObjectiveViewState state,
            out TutorialObjectiveRow row)
        {
            if (!string.IsNullOrWhiteSpace(objectiveId)
                && _statesById.TryGetValue(objectiveId, out state)
                && _rowsById.TryGetValue(objectiveId, out row))
            {
                return true;
            }

            state = null;
            row = null;
            Debug.LogWarning($"[Tutorial Presenter] Unknown objective ID '{objectiveId}'.", this);
            return false;
        }

        private static void RefreshRow(TutorialObjectiveViewState state, TutorialObjectiveRow row)
        {
            TutorialObjectiveData objective = state.Data;
            string text = objective.Text != null ? objective.Text.FallbackText : string.Empty;
            string input = objective.InputAction == TutorialInputAction.None
                ? string.Empty
                : $"[{objective.InputAction}]";

            text = (text ?? string.Empty)
                .Replace("{input}", input)
                .Replace("{current}", state.Current.ToString())
                .Replace("{target}", state.Target.ToString());

            row.Render(text, state.IsCompleted);
        }

        private void ClearRows()
        {
            foreach (TutorialObjectiveRow row in _rowsById.Values)
            {
                if (row != null)
                    Destroy(row.gameObject);
            }

            _rowsById.Clear();
            _statesById.Clear();
        }
    }
}
