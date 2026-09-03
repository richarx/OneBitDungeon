using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorAxeComboData", menuName = "ScriptableObjects/Gladiator/Axe Combo")]
public class GladiatorAxeComboData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone conique")]
    public ConeDamageZone ConeDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Rayon premier coup")]
    [field: SuffixLabel("mètres")]
    public float FirstRadius { get; private set; } = 3.0f;

    [field: SerializeField]
    [field: Range(0.0f, 180.0f)]
    [field: LabelText("Demi-angle premier coup")]
    [field: SuffixLabel("degrés")]
    public float FirstHalfAngle { get; private set; } = 45.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition premier coup")]
    [field: SuffixLabel("secondes")]
    public float FirstSpawnDuration { get; private set; } = 0.35f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage premier coup")]
    [field: SuffixLabel("secondes")]
    public float FirstFillDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Rayon second coup")]
    [field: SuffixLabel("mètres")]
    public float SecondRadius { get; private set; } = 3.0f;

    [field: SerializeField]
    [field: Range(0.0f, 180.0f)]
    [field: LabelText("Demi-angle second coup")]
    [field: SuffixLabel("degrés")]
    public float SecondHalfAngle { get; private set; } = 45.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition second coup")]
    [field: SuffixLabel("secondes")]
    public float SecondSpawnDuration { get; private set; } = 0.35f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage second coup")]
    [field: SuffixLabel("secondes")]
    public float SecondFillDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: LabelText("Se repositionne entre les coups")]
    public bool RepositionBetweenAttacks { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance d'approche premier coup")]
    [field: SuffixLabel("mètres")]
    public float FirstMoveDistance { get; private set; } = 2.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'approche premier coup")]
    [field: SuffixLabel("secondes")]
    public float FirstMoveDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance d'approche second coup")]
    [field: SuffixLabel("mètres")]
    public float SecondMoveDistance { get; private set; } = 2.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'approche second coup")]
    [field: SuffixLabel("secondes")]
    public float SecondMoveDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: LabelText("Animation d'anticipation premier coup")]
    public string FirstAnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Animation d'impact premier coup")]
    public string FirstImpactAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Animation d'impact second coup")]
    public string SecondImpactAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("After-image pendant le premier déplacement")]
    public bool TriggerAfterImageOnFirstSideMove { get; private set; } = true;

    [field: SerializeField]
    [field: LabelText("After-image pendant le second déplacement")]
    public bool TriggerAfterImageOnSecondSideMove { get; private set; } = true;
}
