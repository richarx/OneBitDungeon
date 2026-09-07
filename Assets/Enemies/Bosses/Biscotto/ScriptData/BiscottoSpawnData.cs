using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoSpawnData", menuName = "ScriptableObjects/Biscotto/Spawn Data")]
public sealed class BiscottoSpawnData : ScriptableObject
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
    [field: LabelText("Délai avant début de l'apparition")]
    [field: SuffixLabel("secondes")]
    public float WaitDuration;

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
    [field: MinValue(0.01f)]
    [field: LabelText("Temps de chute")]
    [field: SuffixLabel("secondes")]
    public float FallDuration { get; private set; } = 4.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de recovery")]
    [field: SuffixLabel("secondes")]
    public float RecoveryDuration;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de moquerie")]
    [field: SuffixLabel("secondes")]
    public float LaughDuration;

    [field: SerializeField]
    [field: LabelText("Moquerie")]
    public string LaughAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Saut")]
    public string JumpAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Impact")]
    public string ImpactAnimation { get; private set; }

}
