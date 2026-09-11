using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tutorials
{
    [Serializable]
    public class TutorialObjectiveData
    {
        [SerializeField]
        [LabelText("Objective ID")]
        private string _id = string.Empty;

        [SerializeField]
        [LabelText("Text")]
        [Tooltip("Use {RewiredActionName} for a binding glyph, plus {current} and {target} for progress.")]
        private TutorialText _text = new TutorialText();

        [SerializeField]
        [LabelText("Signal")]
        [Tooltip("Each received signal increments this objective by one.")]
        private TutorialSignalId _signalId;

        [SerializeField]
        [LabelText("Show Progress")]
        private bool _showProgress;

        [SerializeField]
        [ShowIf(nameof(_showProgress))]
        [MinValue(1)]
        [LabelText("Target")]
        private int _target = 1;


        public string Id => _id;

        public TutorialText Text => _text;

        public TutorialSignalId SignalId => _signalId;

        public bool ShowProgress => _showProgress;

        public int Target => Mathf.Max(1, _target);

        public string ListLabel => string.IsNullOrWhiteSpace(_id) ? "New Objective" : _id;
    }
}
