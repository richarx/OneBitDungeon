using System;
using System.Collections.Generic;
using System.Text;
using Rewired;
using Rewired.Glyphs;
using Rewired.Interfaces;
using UnityEngine;

namespace Tools_and_Scripts.RewiredInput
{
    /// <summary>
    /// Uses the character printed by the active keyboard layout when resolving keyboard glyphs.
    /// Rewired bindings remain physical, while an AZERTY W-position binding can display Z.
    /// </summary>
    public sealed class KeyboardLayoutAwareGlyphProvider : GlyphProvider, IGlyphProvider
    {
        private const string KeyboardGlyphPrefix = "controller/keyboard/";
        private const float LayoutCheckInterval = 1f;

        private static readonly KeyboardKeyCode[] _layoutSensitiveKeys =
        {
            KeyboardKeyCode.A, KeyboardKeyCode.B, KeyboardKeyCode.C, KeyboardKeyCode.D,
            KeyboardKeyCode.E, KeyboardKeyCode.F, KeyboardKeyCode.G, KeyboardKeyCode.H,
            KeyboardKeyCode.I, KeyboardKeyCode.J, KeyboardKeyCode.K, KeyboardKeyCode.L,
            KeyboardKeyCode.M, KeyboardKeyCode.N, KeyboardKeyCode.O, KeyboardKeyCode.P,
            KeyboardKeyCode.Q, KeyboardKeyCode.R, KeyboardKeyCode.S, KeyboardKeyCode.T,
            KeyboardKeyCode.U, KeyboardKeyCode.V, KeyboardKeyCode.W, KeyboardKeyCode.X,
            KeyboardKeyCode.Y, KeyboardKeyCode.Z,
            KeyboardKeyCode.Alpha0, KeyboardKeyCode.Alpha1, KeyboardKeyCode.Alpha2,
            KeyboardKeyCode.Alpha3, KeyboardKeyCode.Alpha4, KeyboardKeyCode.Alpha5,
            KeyboardKeyCode.Alpha6, KeyboardKeyCode.Alpha7, KeyboardKeyCode.Alpha8,
            KeyboardKeyCode.Alpha9,
            KeyboardKeyCode.Quote, KeyboardKeyCode.Comma, KeyboardKeyCode.Minus,
            KeyboardKeyCode.Period, KeyboardKeyCode.Slash, KeyboardKeyCode.Semicolon,
            KeyboardKeyCode.Equals, KeyboardKeyCode.LeftBracket,
            KeyboardKeyCode.Backslash, KeyboardKeyCode.RightBracket,
            KeyboardKeyCode.BackQuote
        };

        private readonly StringBuilder _layoutSignatureBuilder = new StringBuilder(128);
        private string _layoutSignature;
        private float _nextLayoutCheckTime;

        bool IGlyphProvider.TryGetGlyph(string key, out object result)
        {
            if (TryGetLayoutGlyphKey(key, out string layoutKey) &&
                glyphs.TryGetValue(layoutKey, out result))
            {
                return true;
            }

            return glyphs.TryGetValue(key, out result);
        }

        protected override void Update()
        {
            base.Update();

            if (!ReInput.isReady || Time.unscaledTime < _nextLayoutCheckTime) return;

            _nextLayoutCheckTime = Time.unscaledTime + LayoutCheckInterval;
            string currentSignature = GetLayoutSignature();

            if (_layoutSignature == null)
            {
                _layoutSignature = currentSignature;
                return;
            }

            if (string.Equals(_layoutSignature, currentSignature, StringComparison.Ordinal)) return;

            _layoutSignature = currentSignature;
            Reload();
        }

        private bool TryGetLayoutGlyphKey(string key, out string layoutKey)
        {
            layoutKey = null;
            if (string.IsNullOrEmpty(key) ||
                !key.StartsWith(KeyboardGlyphPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            string glyphToken = key.Substring(KeyboardGlyphPrefix.Length);
            if (!TryGetKeyCode(glyphToken, out KeyboardKeyCode keyCode)) return false;

            string systemLabel = Keyboard.GetKeyName(keyCode);
            if (!TryGetGlyphToken(systemLabel, out string systemGlyphToken) ||
                string.Equals(glyphToken, systemGlyphToken, StringComparison.Ordinal))
            {
                return false;
            }

            layoutKey = KeyboardGlyphPrefix + systemGlyphToken;
            return true;
        }

        private string GetLayoutSignature()
        {
            _layoutSignatureBuilder.Length = 0;

            for (int i = 0; i < _layoutSensitiveKeys.Length; i++)
            {
                if (i > 0) _layoutSignatureBuilder.Append('\u001f');
                _layoutSignatureBuilder.Append(Keyboard.GetKeyName(_layoutSensitiveKeys[i]));
            }

            return _layoutSignatureBuilder.ToString();
        }

        private static bool TryGetKeyCode(string glyphToken, out KeyboardKeyCode keyCode)
        {
            string enumName;

            switch (glyphToken)
            {
                case "exclamation_point": enumName = nameof(KeyboardKeyCode.Exclaim); break;
                case "double_quote": enumName = nameof(KeyboardKeyCode.DoubleQuote); break;
                case "dollar_sign": enumName = nameof(KeyboardKeyCode.Dollar); break;
                case "left_parenthesis": enumName = nameof(KeyboardKeyCode.LeftParen); break;
                case "right_parenthesis": enumName = nameof(KeyboardKeyCode.RightParen); break;
                case "question_mark": enumName = nameof(KeyboardKeyCode.Question); break;
                case "keypad_slash": enumName = nameof(KeyboardKeyCode.KeypadDivide); break;
                case "keypad_asterisk": enumName = nameof(KeyboardKeyCode.KeypadMultiply); break;
                default: enumName = glyphToken.Replace("_", string.Empty); break;
            }

            return Enum.TryParse(enumName, true, out keyCode);
        }

        private static bool TryGetGlyphToken(string systemLabel, out string glyphToken)
        {
            glyphToken = null;
            if (string.IsNullOrWhiteSpace(systemLabel)) return false;

            string label = systemLabel.Trim();
            if (label.Length != 1) return false;

            char character = char.ToLowerInvariant(label[0]);
            if (character >= 'a' && character <= 'z')
            {
                glyphToken = character.ToString();
                return true;
            }

            if (character >= '0' && character <= '9')
            {
                glyphToken = "alpha_" + character;
                return true;
            }

            switch (character)
            {
                case '!': glyphToken = "exclamation_point"; break;
                case '"': glyphToken = "double_quote"; break;
                case '#': glyphToken = "hash"; break;
                case '$': glyphToken = "dollar_sign"; break;
                case '&': glyphToken = "ampersand"; break;
                case '\'': glyphToken = "quote"; break;
                case '(': glyphToken = "left_parenthesis"; break;
                case ')': glyphToken = "right_parenthesis"; break;
                case '*': glyphToken = "asterisk"; break;
                case '+': glyphToken = "plus"; break;
                case ',': glyphToken = "comma"; break;
                case '-': glyphToken = "minus"; break;
                case '.': glyphToken = "period"; break;
                case '/': glyphToken = "slash"; break;
                case ':': glyphToken = "colon"; break;
                case ';': glyphToken = "semicolon"; break;
                case '<': glyphToken = "less_than"; break;
                case '=': glyphToken = "equals"; break;
                case '>': glyphToken = "greater_than"; break;
                case '?': glyphToken = "question_mark"; break;
                case '@': glyphToken = "at"; break;
                case '[': glyphToken = "left_bracket"; break;
                case '\\': glyphToken = "backslash"; break;
                case ']': glyphToken = "right_bracket"; break;
                case '^': glyphToken = "caret"; break;
                case '_': glyphToken = "underscore"; break;
                case '`': glyphToken = "back_quote"; break;
                default: return false;
            }

            return true;
        }
    }
}
