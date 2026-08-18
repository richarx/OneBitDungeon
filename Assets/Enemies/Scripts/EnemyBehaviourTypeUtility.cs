using System;
using System.Collections.Generic;
using System.Linq;
using Enemies.Scripts.Behaviours;
using UnityEngine;

public static class EnemyBehaviourTypeUtility
{
    public static IEnumerable<Type> GetBehaviourTypes(EnemyController owner)
    {
        return GetBehaviourTypes(owner != null ? owner.gameObject.name : null);
    }

    public static IEnumerable<Type> GetBehaviourTypes(string ownerName)
    {
        IEnumerable<Type> inlineTypes = typeof(IEnemyBehaviour).Assembly
            .GetTypes()
            .Where(type => typeof(IEnemyBehaviour).IsAssignableFrom(type)
                           && !type.IsAbstract
                           && !type.IsInterface
                           && !type.IsGenericType
                           && !typeof(MonoBehaviour).IsAssignableFrom(type));

        string normalizedOwnerName = NormalizeOwnerName(ownerName);
        if (string.IsNullOrEmpty(normalizedOwnerName))
            return inlineTypes;

        return inlineTypes.Where(type => type.Name.IndexOf(normalizedOwnerName, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    public static string NormalizeOwnerName(string ownerName)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
            return string.Empty;

        string normalizedName = ownerName.Trim();
        for (int index = 1; index < normalizedName.Length; index++)
        {
            char character = normalizedName[index];
            if (char.IsUpper(character) || char.IsPunctuation(character) || char.IsWhiteSpace(character))
                return normalizedName.Substring(0, index);
        }

        return normalizedName;
    }
}
