using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PeacockDashAttackData", menuName = "ScriptableObjects/Peacock/Dash Attack Data")]
public class PeacockDashAttackData : ScriptableObject
{
    [field: SerializeField]
    [field: LabelText("Nom du pattern")]
    public string PatternName { get; private set; } = "Dash Attack";

    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone rectangulaire")]
    public GameObject RectangularDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la visée")]
    public float RotationDampening { get; private set; } = 0.08f;

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
    [field: LabelText("S'éloigne du joueur")]
    public bool MoveAwayFromPlayer { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(MoveAwayFromPlayer))]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance")]
    [field: SuffixLabel("mètres")]
    public float MoveDistance { get; private set; } = 2.0f;

    [field: SerializeField]
    [field: ShowIf(nameof(MoveAwayFromPlayer))]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du déplacement")]
    [field: SuffixLabel("secondes")]
    public float MoveDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération finale")]
    [field: SuffixLabel("secondes")]
    public float FinalRecoveryDuration { get; private set; } = 0.8f;

    [field: SerializeField]
    [field: LabelText("Animation d'anticipation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Animation d'impact")]
    public string ImpactAnimation { get; private set; }

    private bool LockBeforeImpactIsValid => LockBeforeImpact >= 0.0f && LockBeforeImpact <= SpawnDuration + FillDuration;
}
