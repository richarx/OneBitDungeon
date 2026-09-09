using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Tutorials
{
    [Serializable]
    [InlineProperty]
    public sealed class TutorialObjectiveData
    {
        [OdinSerialize]
        [LabelText("Objective ID")]
        [Tooltip("Stable identifier used by tutorial logic and saved progress.")]
        private string id = string.Empty;

        [OdinSerialize]
        [LabelText("Text")]
        private TutorialText text = new TutorialText();

        [OdinSerialize]
        [LabelText("Input Action")]
        private TutorialInputAction inputAction = TutorialInputAction.None;

        [OdinSerialize]
        [LabelText("Show Progress")]
        private bool showProgress;

        [OdinSerialize]
        [ShowIf(nameof(showProgress))]
        [MinValue(1)]
        [LabelText("Target")]
        private int target = 1;

        [OdinSerialize]
        [LabelText("Required")]
        [Tooltip("A tutorial may finish only after all of its required objectives are complete.")]
        private bool required = true;

        public string Id => id;

        public TutorialText Text => text;

        public TutorialInputAction InputAction => inputAction;

        public bool ShowProgress => showProgress;

        public int Target => Mathf.Max(1, target);

        public bool Required => required;

        public string ListLabel => string.IsNullOrWhiteSpace(id) ? "New Objective" : id;
    }
}
