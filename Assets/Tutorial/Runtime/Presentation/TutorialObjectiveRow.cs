using PrimeTween;
using Rewired.Glyphs.UnityUI;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tutorials
{
    public sealed class TutorialObjectiveRow : MonoBehaviour
    {
        [TitleGroup("References")]
        [SerializeField, Required]
        private Image _checkboxImage;

        [TitleGroup("References")]
        [SerializeField, Required]
        private TextMeshProUGUI _objectiveText;

        [TitleGroup("References")]
        [SerializeField, Required]
        [Tooltip("Parses Rewired tags and displays the current binding inline in the objective text.")]
        private UnityUITextMeshProGlyphHelper _glyphHelper;

        [TitleGroup("Checkbox")]
        [SerializeField, Required]
        private Sprite _uncheckedSprite;

        [TitleGroup("Checkbox")]
        [SerializeField, Required]
        private Sprite _checkedSprite;

        public void Render(string text, bool isCompleted)
        {
            SetText(text);
            SetCompleted(isCompleted);
        }

        public void SetText(string text)
        {
            string value = text ?? string.Empty;

            if (_glyphHelper != null)
            {
                _glyphHelper.text = value;
                return;
            }

            if (_objectiveText != null)
                _objectiveText.text = value;
        }

        public void SetCompleted(bool isCompleted)
        {
            if (_checkboxImage == null)
                return;

            Sprite targetSprite = isCompleted ? _checkedSprite : _uncheckedSprite;
            if (targetSprite != null)
                _checkboxImage.sprite = targetSprite;

            if (isCompleted)
                Sequence.Create()
                    .Chain(Tween.PunchScale(_checkboxImage.transform, Vector3.one, 0.15f));
        }
    }
}
