using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Tutorials
{
    public abstract class TutorialData : SerializedScriptableObject
    {
        [TitleGroup("Identity")]
        [OdinSerialize]
        [LabelText("Tutorial ID")]
        [Tooltip("Stable identifier used by sequencing and saved progress.")]
        private string tutorialId = string.Empty;

        [TitleGroup("Identity")]
        [OdinSerialize]
        [LabelText("Editor Label")]
        private string editorLabel = string.Empty;

        [TitleGroup("Objectives")]
        [OdinSerialize]
        [LabelText("Objectives")]
        [ListDrawerSettings(
            DraggableItems = true,
            ShowFoldout = true,
            ShowIndexLabels = false,
            ListElementLabelName = nameof(TutorialObjectiveData.ListLabel))]
        private List<TutorialObjectiveData> objectives = new List<TutorialObjectiveData>();

        public string TutorialId => tutorialId;

        public string EditorLabel => string.IsNullOrWhiteSpace(editorLabel) ? name : editorLabel;

        public IReadOnlyList<TutorialObjectiveData> Objectives => objectives;
    }
}
