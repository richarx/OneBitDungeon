using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tutorials
{
    [Serializable]
    [InlineProperty]
    public class TutorialObjectiveData
    {
        [SerializeField]
        [LabelText("Objective ID")]
        private string _id = string.Empty;

        [SerializeField]
        [LabelText("Text")]
        private TutorialText _text = new TutorialText();

        [SerializeField]
        [LabelText("Input Action")]
        private TutorialInputAction _inputAction = TutorialInputAction.None;

        [SerializeField]
        [LabelText("Show Progress")]
        private bool _showProgress;

        [SerializeField]
        [ShowIf(nameof(_showProgress))]
        [MinValue(1)]
        [LabelText("Target")]
        private int _target = 1;

        [SerializeField]
        [LabelText("Required")]
        [Tooltip("A tutorial may finish only after all of its required objectives are complete.")]
        private bool _required = true;

        public string Id => _id;

        public TutorialText Text => _text;

        public TutorialInputAction InputAction => _inputAction;

        public bool ShowProgress => _showProgress;

        public int Target => Mathf.Max(1, _target);

        public bool Required => _required;

        public string ListLabel => string.IsNullOrWhiteSpace(_id) ? "New Objective" : _id;
    }
}
