using System;
using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "CommonPlayAnimationData", menuName = "ScriptableObjects/Common Behaviours/Play Animation")]
public class CommonPlayAnimationData : SerializedScriptableObject
{
    [field: SerializeField]
    [field: LabelText("Animation")]
    public string Animation { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de l'animation")]
    [field: SuffixLabel("secondes")]
    public float AnimationDuration;

    [field: SerializeField]
    [field: MinValue(0)]
    [field: LabelText("Dégats maximum toléré avant de stopper l'animation")]
    [field: SuffixLabel("hp")]
    public int DamageThreshold;

    [field: SerializeField]
    [field: LabelText("Se déplace vers centre de l'arène")]
    public bool MoveToArenaCenter { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(MoveToArenaCenter))]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du déplacement")]
    [field: SuffixLabel("secondes")]
    public float MoveDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: ShowIf(nameof(MoveToArenaCenter))]
    [field: LabelText("Animation")]
    public string MoveAnimation { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(MoveToArenaCenter))]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = false;

    [field: SerializeField]
    [field: LabelText("enchaine une animation après celle ci")]
    public bool IsChainingAnimation { get; private set; }

    [ShowIf(nameof(IsChainingAnimation))]
    [OdinSerialize]
    [LabelText("Chained Animation")]
    [HideReferenceObjectPicker]
    [TypeFilter(nameof(GetInlineBehaviourTypes))]
    public IEnemyBehaviour chainedBehaviour;

    private IEnumerable<Type> GetInlineBehaviourTypes()
    {
        return EnemyBehaviourTypeUtility.GetBehaviourTypes("PlayAnimation");
    }
}
