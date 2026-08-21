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
    [field: MinValue(0.0f)]
    [field: MaxValue(1.0f)]
    [field: LabelText("Ratio montée/descente")]
    public float RatioAscentToFall { get; private set; } = 0.6f;

    [ShowInInspector]
    [ReadOnly]
    [LabelText("Temps de montée ")]
    [SuffixLabel("secondes")]
    public float AscentDuration => RatioAscentToFall * Mathf.Max(0.0f, SpawnDuration + FillDuration - LockBeforeImpact);

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Hauteur du saut")]
    [field: SuffixLabel("mètres")]
    public float JumpHeight { get; private set; } = 2.0f;

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
