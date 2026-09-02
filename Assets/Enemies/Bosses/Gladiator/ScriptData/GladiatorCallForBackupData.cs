using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorCallForBackupData", menuName = "ScriptableObjects/Gladiator/Call For Backup")]
public class GladiatorCallForBackupData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de Corum")]
    public GameObject CorumPrefab;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Nombre de Corums à spawn")]
    public float CorumCount;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Delai entre chaque spawn")]
    [field: SuffixLabel("secondes")]
    public float TimeBeetweenSpawns;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Délai avant spawn")]
    [field: SuffixLabel("secondes")]
    public float DelayBeforeSpawn;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Delay après spawn")]
    [field: SuffixLabel("secondes")]
    public float DelayAfterSpawn;

    [field: SerializeField]
    [field: LabelText("Se déplace vers le fond de l'arene")]
    public bool IsMovingToBackOfArena { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(IsMovingToBackOfArena))]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du déplacement")]
    [field: SuffixLabel("secondes")]
    public float MoveDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: LabelText("Animation d'anticipation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = true;
}
