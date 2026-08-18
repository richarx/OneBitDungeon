using System;
using System.Collections.Generic;
using System.Linq;
using Enemies.Scripts.Behaviours;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
[InlineProperty]
public class OdinEnemyPhase
{
    [OdinSerialize]
    [LabelText("Seuil de transition (PV)")]
    public int healthThresholdToTriggerTransition;

    [OdinSerialize]
    [LabelText("Transition")]
    [HideReferenceObjectPicker]
    [TypeFilter(nameof(GetInlineBehaviourTypes))]
    public IEnemyBehaviour transitionBehaviour;

    [OdinSerialize]
    [LabelText("Attaques")]
    [ListDrawerSettings(ShowFoldout = true)]
    [TypeFilter(nameof(GetInlineBehaviourTypes))]
    public List<IEnemyBehaviour> phaseBehaviours = new List<IEnemyBehaviour>();



    public List<IEnemyBehaviour> GetBehaviours()
    {
        List<IEnemyBehaviour> behaviours = new List<IEnemyBehaviour>();

        if (phaseBehaviours == null)
            return behaviours;

        foreach (IEnemyBehaviour behaviour in phaseBehaviours)
        {
            if (behaviour == null)
                continue;

            behaviour.SetSubBehaviourState(false);
            behaviours.Add(behaviour);
        }

        return behaviours;
    }

    private static IEnumerable<Type> GetInlineBehaviourTypes()
    {
        return typeof(IEnemyBehaviour).Assembly
            .GetTypes()
            .Where(type => typeof(IEnemyBehaviour).IsAssignableFrom(type)
                           && !type.IsAbstract
                           && !type.IsInterface
                           && !type.IsGenericType
                           && !typeof(MonoBehaviour).IsAssignableFrom(type));
    }
}
