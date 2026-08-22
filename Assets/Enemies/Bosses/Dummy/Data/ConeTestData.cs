using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ConeTestData", menuName = "ScriptableObjects/Dummy/Cone Test Data")]
public sealed class ConeTestData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone conique")]
    public ConeDamageZone ConeDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Intervalle d'attaque")]
    [field: SuffixLabel("secondes")]
    public float AttackInterval { get; private set; } = 3.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Rayon")]
    [field: SuffixLabel("mètres")]
    public float Radius { get; private set; } = 3.0f;

    [field: SerializeField]
    [field: Range(0.0f, 360.0f)]
    [field: LabelText("Angle d'ouverture")]
    [field: SuffixLabel("degrés")]
    public float OpeningAngle { get; private set; } = 90.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    [field: SuffixLabel("secondes")]
    public float SpawnDuration { get; private set; } = 0.3f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage")]
    [field: SuffixLabel("secondes")]
    public float FillDuration { get; private set; } = 1.0f;
}
