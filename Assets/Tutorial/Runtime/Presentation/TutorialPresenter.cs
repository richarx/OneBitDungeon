using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools_and_Scripts.RewiredInput;
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

        private readonly HashSet<TutorialInputAction> _missingInputActionWarnings =
            new HashSet<TutorialInputAction>();

        private ITutorialTextResolver _textResolver = new FallbackTutorialTextResolver();

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

        public void SetTextResolver(ITutorialTextResolver textResolver)
        {
            _textResolver = textResolver ?? new FallbackTutorialTextResolver();
            RefreshAllRows();
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

        private void RefreshRow(TutorialObjectiveViewState state, TutorialObjectiveRow row)
        {
            TutorialObjectiveData objective = state.Data;
            string template = _textResolver.Resolve(objective.Text);
            string input = ResolveInputTag(objective.InputAction);
            string text = TutorialTextFormatter.Format(
                template,
                input,
                state.Current,
                state.Target);

            row.Render(text, state.IsCompleted);
        }

        private string ResolveInputTag(TutorialInputAction action)
        {
            if (action == TutorialInputAction.None)
                return string.Empty;

            if (!TutorialRewiredInputActionMap.TryGetActionNames(
                    action,
                    out string primaryActionName,
                    out string secondaryActionName))
            {
                if (_missingInputActionWarnings.Add(action))
                {
                    Debug.LogWarning(
                        $"[Tutorial Presenter] No Rewired action is mapped for tutorial action '{action}'.",
                        this);
                }

                return $"[{action}]";
            }

            int playerId = RewiredInputRuntime.Instance != null
                ? RewiredInputRuntime.Instance.PlayerId
                : 0;

            if (string.IsNullOrEmpty(secondaryActionName))
            {
                return $"<rewiredElement type=\"glyphOrText\" playerId={playerId} "
                       + $"actionName=\"{primaryActionName}\">";
            }

            return $"<rewiredElement type=\"glyphOrText\" playerId={playerId} "
                   + $"actionName=\"{primaryActionName}\" "
                   + $"actionName2=\"{secondaryActionName}\">";
        }

        private void RefreshAllRows()
        {
            foreach (KeyValuePair<string, TutorialObjectiveViewState> entry in _statesById)
            {
                if (_rowsById.TryGetValue(entry.Key, out TutorialObjectiveRow row) && row != null)
                    RefreshRow(entry.Value, row);
            }
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
