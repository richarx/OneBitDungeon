using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoSlamData", menuName = "ScriptableObjects/Biscotto/Slam Data")]
public class BiscottoSlamData : SerializedScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone circulaire")]
    public CircleDamageZone CircleDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Rayon")]
    [field: SuffixLabel("mètres")]
    public float Radius { get; private set; } = 0.16f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    [field: SuffixLabel("secondes")]
    public float SpawnDuration { get; private set; } = 0.3f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de fill")]
    [field: SuffixLabel("secondes")]
    public float FillDuration { get; private set; } = 0.9f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération")]
    [field: SuffixLabel("secondes")]
    public float RecoveryDuration { get; private set; } = 0.8f;

    [field: SerializeField]
    [field: LabelText("Se déplace vers le centre de l'arène")]
    public bool MoveToArenaCenter { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(MoveToArenaCenter))]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du déplacement")]
    [field: SuffixLabel("secondes")]
    public float MoveDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: ShowIf(nameof(MoveToArenaCenter))]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = true;

    [Title("Animations")]
    [field: SerializeField]
    [field: LabelText("Anticipation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Impact")]
    public string ImpactAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("enchaine une attaque après celle ci")]
    public bool IsChainingBehaviour { get; private set; }

    [ShowIf(nameof(IsChainingBehaviour))]
    [OdinSerialize]
    [LabelText("attaque chainée")]
    [HideReferenceObjectPicker]
    [TypeFilter(nameof(GetInlineBehaviourTypes))]
    public IEnemyBehaviour chainedBehaviour;

    private IEnumerable<Type> GetInlineBehaviourTypes()
    {
        return EnemyBehaviourTypeUtility.GetBehaviourTypes("Biscotto");
    }
}
