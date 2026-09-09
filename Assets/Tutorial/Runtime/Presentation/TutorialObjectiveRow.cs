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
            if (_objectiveText != null)
                _objectiveText.text = text ?? string.Empty;
        }

        public void SetCompleted(bool isCompleted)
        {
            if (_checkboxImage == null)
                return;

            Sprite targetSprite = isCompleted ? _checkedSprite : _uncheckedSprite;
            if (targetSprite != null)
                _checkboxImage.sprite = targetSprite;
        }
    }
}
