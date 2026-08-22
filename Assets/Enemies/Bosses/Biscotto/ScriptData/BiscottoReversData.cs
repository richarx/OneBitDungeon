using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoReversData", menuName = "ScriptableObjects/Biscotto/Revers Data")]
public sealed class BiscottoReversData : ScriptableObject
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

    [Title("Mouvement d'approche")]
    [field: SerializeField]
    [field: LabelText("Côté choisi")]
    public BiscottoSideSelection SideSelection { get; private set; } = BiscottoSideSelection.Random;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance latérale")]
    [field: SuffixLabel("mètres")]
    public float SideMoveDistance { get; private set; } = 2.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du déplacement")]
    [field: SuffixLabel("secondes")]
    [field: SuffixLabel("secondes")]
    public float SideMoveDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = true;

    [Title("Télégraphe")]
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

    [Title("Résultats")]
    [field: SerializeField]
    [field: MinValue(0)]
    [field: LabelText("Dégâts retournés à Biscotto")]
    public int SelfDamage { get; private set; } = 50;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Perte d'arrogance sur esquive de lâche")]
    public float EarlyDodgeArroganceLoss { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération si le joueur est touché")]
    [field: SuffixLabel("secondes")]
    public float HitRecoveryDuration { get; private set; } = 0.8f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération sur esquive de lâche")]
    [field: SuffixLabel("secondes")]
    public float EarlyDodgeRecoveryDuration { get; private set; } = 0.45f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Stun après retour")]
    [field: SuffixLabel("secondes")]
    public float ReflectedStunDuration { get; private set; } = 1.6f;

    [Title("Animations")]
    [field: SerializeField]
    [field: LabelText("Invitation")]
    public string InvitationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Revers")]
    public string ImpactAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Réussite de Biscotto")]
    public string HitPlayerAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Provocation esquive précoce")]
    public string EarlyDodgeAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Revers retourné")]
    public string ReflectedAnimation { get; private set; }
}
