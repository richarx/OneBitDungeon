using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Tutorials
{
    [CreateAssetMenu(fileName = "New Tutorial Data", menuName = "Tutorials/Tutorial Data")]
    public class TutorialData : ScriptableObject
    {
        [TitleGroup("Identity")]
        [SerializeField]
        [LabelText("Tutorial ID")]
        [Tooltip("Stable identifier used by sequencing and saved progress.")]
        private string tutorialId = string.Empty;

        [TitleGroup("Identity")]
        [SerializeField]
        [LabelText("Editor Label")]
        private string editorLabel = string.Empty;

        [TitleGroup("Objectives")]
        [SerializeField]
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
