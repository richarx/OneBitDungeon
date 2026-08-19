using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
[InlineProperty]
public sealed class BiscottoPunchStep
{
    [OdinSerialize]
    [LabelText("Nom de l'étape")]
    public string StepName { get; private set; } = "Coup";

    [OdinSerialize]
    [LabelText("Prefab de zone (optionnel)")]
    [Tooltip("Remplace le prefab défini sur le comportement pour cette étape uniquement.")]
    public GameObject RectangularDamageZonePrefabOverride { get; private set; }

    [OdinSerialize]
    [LabelText("Animation d'anticipation")]
    public string AnticipationAnimation { get; private set; }

    [OdinSerialize]
    [LabelText("Animation d'impact")]
    public string ImpactAnimation { get; private set; }

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Délai avant l'étape")]
    public float DelayBeforeStep { get; private set; }

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée d'apparition")]
    public float SpawnDuration { get; private set; } = 0.3f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée de remplissage")]
    public float FillDuration { get; private set; } = 0.8f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Verrouillage avant impact")]
    [SuffixLabel("secondes")]
    public float LockBeforeImpact { get; private set; } = 0.3f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Anticipation animation d'impact")]
    [SuffixLabel("secondes")]
    public float ImpactAnimationLeadTime { get; private set; } = 0.1f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Délai après impact")]
    public float DelayAfterImpact { get; private set; } = 0.2f;

    [OdinSerialize]
    [LabelText("Se déplacer à côté du joueur")]
    public bool MoveBesidePlayer { get; private set; }

    [OdinSerialize]
    [ShowIf(nameof(MoveBesidePlayer))]
    [LabelText("Côté choisi")]
    public BiscottoSideSelection SideSelection { get; private set; } = BiscottoSideSelection.Random;

    [OdinSerialize]
    [ShowIf(nameof(MoveBesidePlayer))]
    [MinValue(0.0f)]
    [LabelText("Distance latérale")]
    public float SideMoveDistance { get; private set; } = 2.0f;

    [OdinSerialize]
    [ShowIf(nameof(MoveBesidePlayer))]
    [MinValue(0.0f)]
    [LabelText("Durée du déplacement")]
    public float SideMoveDuration { get; private set; } = 0.25f;
}
