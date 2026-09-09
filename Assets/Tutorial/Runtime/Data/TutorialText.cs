using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Tutorials
{
    [Serializable]
    [InlineProperty]
    public sealed class TutorialText
    {
        [OdinSerialize]
        [LabelText("Localization Key")]
        [Tooltip("Stable key reserved for the future localization table.")]
        private string localizationKey = string.Empty;

        [OdinSerialize]
        [LabelText("Fallback Text")]
        [TextArea(2, 5)]
        [Tooltip("Text displayed while no localization provider is installed.")]
        private string fallbackText = string.Empty;

        public string LocalizationKey => localizationKey;

        public string FallbackText => fallbackText;
    }
}
