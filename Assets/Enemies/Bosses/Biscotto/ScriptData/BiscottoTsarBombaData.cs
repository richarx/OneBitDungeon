using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoTsarBombaData", menuName = "ScriptableObjects/Biscotto/Tsar Bomba Data")]
public sealed class BiscottoTsarBombaData : ScriptableObject
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
    [field: LabelText("Verrouillage avant impact")]
    [field: ValidateInput(nameof(LockBeforeImpactIsValid), "Le verrouillage avant impact doit être inférieur ou égal à la durée totale d'apparition et de fill.")]
    [field: SuffixLabel("secondes")]
    public float LockBeforeImpact { get; private set; } = 0.32f;

    [field: SerializeField]
    [field: MinValue(0.01f)]
    [field: LabelText("Temps de montée")]
    [field: SuffixLabel("secondes")]
    public float AscentDuration { get; private set; } = 5.0f;

    [field: SerializeField]
    [field: MinValue(0.01f)]
    [field: LabelText("Temps de chute")]
    [field: SuffixLabel("secondes")]
    public float FallDuration { get; private set; } = 4.0f;

    [ShowInInspector]
    [ReadOnly]
    [LabelText("Temps de suivi")]
    [SuffixLabel("secondes")]
    public float TrackingDuration => Mathf.Max(0.0f, SpawnDuration + FillDuration - LockBeforeImpact);

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération")]
    [field: SuffixLabel("secondes")]
    public float RecoveryDuration { get; private set; } = 0.8f;

    [Title("Animations")]
    [field: SerializeField]
    [field: LabelText("Anticipation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Saut")]
    public string JumpAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Impact")]
    public string ImpactAnimation { get; private set; }

    private bool LockBeforeImpactIsValid => LockBeforeImpact >= 0.0f && LockBeforeImpact <= SpawnDuration + FillDuration;
}
