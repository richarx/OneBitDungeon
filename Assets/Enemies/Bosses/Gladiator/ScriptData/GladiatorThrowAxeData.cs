using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorThrowAxeData", menuName = "ScriptableObjects/Gladiator/Throw Axe Data")]
public class GladiatorThrowAxeData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone rectangulaire")]
    public GameObject RectangleDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de hache")]
    public AxeController AxePrefab;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    [field: SuffixLabel("secondes")]
    public float SpawnDuration;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage")]
    [field: SuffixLabel("secondes")]
    public float FillDuration;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Verrouillage avant impact")]
    [field: ValidateInput(nameof(LockBeforeImpactIsValid), "Le verrouillage avant impact doit être inférieur ou égal à la durée totale d'apparition et de fill.")]
    [field: SuffixLabel("secondes")]
    public float LockBeforeImpact { get; private set; } = 0.32f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la visée")]
    public float RotationDampening;

    [field: SerializeField]
    [field: MinValue(-0.1f)]
    [field: LabelText("Durée de l'animation de lancé de hache")]
    [field: SuffixLabel("secondes")]
    public float ThrowAnimationDuration;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Distance parcourue par la hache")]
    [field: SuffixLabel("mètres")]
    public float AxeFlyDistance;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Durée de déplacement de la hache")]
    [field: SuffixLabel("secondes")]
    public float AxeFlyDuration;

    [field: SerializeField]
    [field: LabelText("S'éloigne du joueur")]
    public bool MoveAwayFromPlayer { get; private set; }

    [field: SerializeField]
    [field: HideIf(nameof(MoveAwayFromPlayer))]
    [field: LabelText("Se déplace vers le fond de l'arene")]
    public bool MoveToRandomPosition { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(MoveAwayFromPlayer))]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance de déplacement")]
    [field: SuffixLabel("mètres")]
    public float MoveDistance { get; private set; } = 2.0f;

    [field: SerializeField]
    [field: ShowIf(nameof(MoveAwayFromPlayer))]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du déplacement")]
    [field: SuffixLabel("secondes")]
    public float MoveDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: LabelText("Animation d'anticipation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Animation d'impact")]
    public string ImpactAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = true;

    private bool LockBeforeImpactIsValid => LockBeforeImpact >= 0.0f && LockBeforeImpact <= SpawnDuration + FillDuration;
}
