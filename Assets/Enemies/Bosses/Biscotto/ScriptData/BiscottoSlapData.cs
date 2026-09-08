using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoSlapData", menuName = "ScriptableObjects/Biscotto/Slap Data")]
public class BiscottoSlapData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone conique")]
    public ConeDamageZone ConeDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Rayon")]
    [field: SuffixLabel("mètres")]
    public float Radius { get; private set; } = 3.0f;

    [field: SerializeField]
    [field: Range(0.0f, 180.0f)]
    [field: LabelText("Demi-angle")]
    [field: SuffixLabel("degrés")]
    public float HalfAngle { get; private set; } = 45.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    [field: SuffixLabel("secondes")]
    public float SpawnDuration { get; private set; } = 0.35f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage")]
    [field: SuffixLabel("secondes")]
    public float FillDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Verrouillage avant impact")]
    [field: SuffixLabel("secondes")]
    public float LockBeforeImpact { get; private set; } = 0.3f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la visée")]
    public float RotationDampening { get; private set; } = 0.08f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance d'approche")]
    [field: SuffixLabel("mètres")]
    public float MoveDistance { get; private set; } = 2.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'approche")]
    [field: SuffixLabel("secondes")]
    public float MoveDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = true;

    [Title("Animations")]
    [field: SerializeField]
    [field: LabelText("Anticipation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Revers")]
    public string ImpactAnimation { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de recovery-")]
    [field: SuffixLabel("secondes")]
    public float RecoveryDuration { get; private set; } = 1.0f;
}
