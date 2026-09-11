using System.Collections;
using System.Collections.Generic;
using Rewired;
using Tools_and_Scripts.RewiredInput;
using UnityEngine;

public static class RewiredTool
{
    // Static utility methods for RewiredTool can be added here

    public static string ResolveRewiredActionTokens(string text, int playerId = -1)
    {
        if (string.IsNullOrEmpty(text) || !ReInput.isReady)
            return text;

        if (playerId == -1)
        {
            playerId = RewiredInputRuntime.Instance != null
                ? RewiredInputRuntime.Instance.PlayerId
                : 0;
        }

        IList<InputAction> actions = ReInput.mapping.Actions;
        foreach (InputAction action in actions)
        {
            string token = $"{{{action.name}}}";
            if (!text.Contains(token))
                continue;

            string rewiredTag = $"<rewiredElement type=\"glyphOrText\" "
                                + $"playerId={playerId} actionId={action.id}>";
            text = text.Replace(token, rewiredTag);
        }

        return text;
    }
}
