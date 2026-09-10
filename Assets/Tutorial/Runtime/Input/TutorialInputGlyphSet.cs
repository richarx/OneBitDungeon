using System;
using Sirenix.OdinInspector;
using Tools_and_Scripts;
using UnityEngine;

namespace Tutorials
{
    [Serializable]
    [InlineProperty]
    public sealed class TutorialInputGlyphSet
    {
        [SerializeField]
        [LabelText("Action")]
        private TutorialInputAction _action = TutorialInputAction.None;

        [SerializeField]
        [LabelText("Keyboard Glyph")]
        private string _keyboardGlyphName = string.Empty;

        [SerializeField]
        [LabelText("Gamepad Glyph")]
        private string _gamepadGlyphName = string.Empty;

        public TutorialInputAction Action => _action;

        public string ListLabel => _action.ToString();

        public string GetGlyphName(InputType inputType)
        {
            return inputType == InputType.Gamepad
                ? _gamepadGlyphName
                : _keyboardGlyphName;
        }
    }
}
