using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tutorials
{
    [Serializable]
    [InlineProperty]
    public sealed class TutorialText
    {
        [SerializeField]
        [LabelText("Localization Key")]
        [Tooltip("Stable key reserved for the future localization table.")]
        private string localizationKey = string.Empty;

        [SerializeField]
        [LabelText("Fallback Text")]
        [TextArea(2, 5)]
        [Tooltip("Text displayed while no localization provider is installed.")]
        private string fallbackText = string.Empty;

        public string LocalizationKey => localizationKey;

        public string FallbackText => fallbackText;
    }
}
