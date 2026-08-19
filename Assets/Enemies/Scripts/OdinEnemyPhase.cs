using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
[InlineProperty]
public class OdinEnemyPhase
{
    [NonSerialized] private EnemyController owner;

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

    public void BindOwner(EnemyController owner)
    {
        this.owner = owner;
    }

    private IEnumerable<Type> GetInlineBehaviourTypes()
    {
        return EnemyBehaviourTypeUtility.GetBehaviourTypes(owner);
    }
}
