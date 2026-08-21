using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[InlineProperty]
public sealed class BiscottoPunchStep
{
    [field: SerializeField]
    [field: LabelText("Nom de l'étape")]
    public string StepName { get; private set; } = "Coup";

    [field: SerializeField]
    [field: LabelText("Prefab de zone (optionnel)")]
    [field: Tooltip("Remplace le prefab défini sur le comportement pour cette étape uniquement.")]
    public GameObject RectangularDamageZonePrefabOverride { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Largeur de la zone")]
    [field: Tooltip("Largeur totale de la zone de dégâts, perpendiculaire au coup.")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneWidth { get; private set; } = 4.0f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Longueur de la zone")]
    [field: Tooltip("Longueur totale de la zone de dégâts, dans l'axe du coup.")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneLength { get; private set; } = 6.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Offset latéral de la zone")]
    [field: Tooltip("Distance entre Biscotto et la zone, perpendiculairement à la direction du joueur.")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneSideOffset { get; private set; }

    [field: SerializeField]
    [field: LabelText("Côté du coup")]
    [field: Tooltip("Choisit le côté gauche ou droit de Biscotto lorsqu'il fait face au joueur.")]
    public BiscottoPunchSide DamageZoneSide { get; private set; } = BiscottoPunchSide.Right;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    [field: SuffixLabel("secondes")]
    public float SpawnDuration { get; private set; } = 0.3f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage")]
    [field: SuffixLabel("secondes")]
    public float FillDuration { get; private set; } = 0.8f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Verrouillage avant impact")]
    [field: SuffixLabel("secondes")]
    [field: ValidateInput(nameof(LockBeforeImpactIsValid), "Le verrouillage avant impact doit être inférieur ou égal à la durée totale d'apparition et de fill.")]
    public float LockBeforeImpact { get; private set; } = 0.3f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Délai après impact")]
    [field: SuffixLabel("secondes")]
    public float DelayAfterImpact { get; private set; } = 0.2f;

    [field: SerializeField]
    [field: LabelText("Se déplacer à côté du joueur")]
    public bool MoveBesidePlayer { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(MoveBesidePlayer))]
    [field: LabelText("Côté choisi")]
    public BiscottoSideSelection SideSelection { get; private set; } = BiscottoSideSelection.Random;

    [field: SerializeField]
    [field: ShowIf(nameof(MoveBesidePlayer))]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance latérale")]
    [field: SuffixLabel("mètres")]
    public float SideMoveDistance { get; private set; } = 2.0f;

    [field: SerializeField]
    [field: ShowIf(nameof(MoveBesidePlayer))]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du déplacement")]
    [field: SuffixLabel("secondes")]
    public float SideMoveDuration { get; private set; } = 0.25f;


    [field: SerializeField]
    [field: LabelText("Animation d'anticipation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Animation d'impact")]
    public string ImpactAnimation { get; private set; }

    private bool LockBeforeImpactIsValid => LockBeforeImpact >= 0.0f && LockBeforeImpact <= SpawnDuration + FillDuration;
}
