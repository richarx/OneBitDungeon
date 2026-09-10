using System.Collections.Generic;
using Player.Scripts;
using Sirenix.OdinInspector;
using Tools_and_Scripts;
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

        [TitleGroup("Input Glyphs")]
        [SerializeField]
        private TutorialInputGlyphDatabase _glyphDatabase;

        private readonly Dictionary<string, TutorialObjectiveViewState> _statesById =
            new Dictionary<string, TutorialObjectiveViewState>();

        private readonly Dictionary<string, TutorialObjectiveRow> _rowsById =
            new Dictionary<string, TutorialObjectiveRow>();

        private readonly HashSet<string> _missingGlyphWarnings = new HashSet<string>();

        private ITutorialTextResolver _textResolver = new FallbackTutorialTextResolver();
        private InputType _currentInputType = InputType.Keyboard;

        private void OnEnable()
        {
            InputPacker.OnChangeInputType.RemoveListener(HandleInputTypeChanged);
            InputPacker.OnChangeInputType.AddListener(HandleInputTypeChanged);
            RefreshCurrentInputType();
            RefreshAllRows();
        }

        private void OnDisable()
        {
            InputPacker.OnChangeInputType.RemoveListener(HandleInputTypeChanged);
        }

        public void ShowObjectives(IReadOnlyList<TutorialObjectiveData> objectives)
        {
            ClearRows();
            RefreshCurrentInputType();

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
                row.SetSpriteAsset(_glyphDatabase != null ? _glyphDatabase.SpriteAsset : null);
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

            if (_glyphDatabase != null
                && _glyphDatabase.TryGetGlyphName(action, _currentInputType, out string glyphName))
            {
                return $"<sprite name=\"{glyphName}\">";
            }

            string warningKey = $"{_currentInputType}:{action}";
            if (_missingGlyphWarnings.Add(warningKey))
            {
                Debug.LogWarning(
                    $"[Tutorial Presenter] Missing {_currentInputType} glyph for action '{action}'.",
                    this);
            }

            return $"[{action}]";
        }

        private void HandleInputTypeChanged(InputType inputType)
        {
            _currentInputType = inputType;
            RefreshAllRows();
        }

        private void RefreshCurrentInputType()
        {
            PlayerStateMachine player = PlayerStateMachine.instance;
            if (player != null && player.inputPackage != null)
                _currentInputType = player.inputPackage.lastInputType;
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
