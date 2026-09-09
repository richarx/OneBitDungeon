using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using Tools_and_Scripts;
using UnityEngine;

namespace Tutorials
{
    [CreateAssetMenu(
        fileName = "TutorialInputGlyphDatabase",
        menuName = "Tutorial/Input Glyph Database")]
    public sealed class TutorialInputGlyphDatabase : ScriptableObject
    {
        [TitleGroup("TextMeshPro")]
        [SerializeField]
        [Required]
        private TMP_SpriteAsset _spriteAsset;

        [TitleGroup("Glyphs")]
        [SerializeField]
        [ListDrawerSettings(
            DraggableItems = true,
            ShowFoldout = true,
            ShowIndexLabels = false,
            ListElementLabelName = nameof(TutorialInputGlyphSet.ListLabel))]
        private List<TutorialInputGlyphSet> _glyphs = new List<TutorialInputGlyphSet>();

        public TMP_SpriteAsset SpriteAsset => _spriteAsset;

        public bool TryGetGlyphName(
            TutorialInputAction action,
            InputType inputType,
            out string glyphName)
        {
            foreach (TutorialInputGlyphSet glyphSet in _glyphs)
            {
                if (glyphSet == null || glyphSet.Action != action)
                    continue;

                glyphName = glyphSet.GetGlyphName(inputType);
                return !string.IsNullOrWhiteSpace(glyphName);
            }

            glyphName = string.Empty;
            return false;
        }
    }
}
