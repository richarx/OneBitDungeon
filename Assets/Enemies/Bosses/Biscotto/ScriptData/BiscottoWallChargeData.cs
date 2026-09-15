using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoWallChargeData", menuName = "ScriptableObjects/Biscotto/Wall Charge Data")]
public sealed class BiscottoWallChargeData : ScriptableObject
{
    [Title("Zone de combat")]
    [field: SerializeField]
    [field: LabelText("Centre de la salle")]
    [field: Tooltip("Coordonnées X/Z du centre de la zone jouable.")]
    public Vector2 ArenaCenter { get; private set; } = Vector2.zero;

    [field: SerializeField]
    [field: MinValue(0.01f)]
    [field: LabelText("Demi-taille de la salle")]
    [field: Tooltip("Demi-largeur sur X et demi-longueur sur Z.")]
    public Vector2 ArenaHalfSize { get; private set; } = new Vector2(9.0f, 9.0f);

    [field: SerializeField]
    [field: LabelText("Position au fond")]
    [field: Tooltip("Position X/Z où Biscotto se place avant de viser.")]
    public Vector2 BackPosition { get; private set; } = new Vector2(0.0f, 8.0f);

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Marge devant le mur")]
    [field: SuffixLabel("mètres")]
    public float WallInset { get; private set; } = 0.75f;

    [Title("Repositionnement")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée vers le fond")]
    [field: SuffixLabel("secondes")]
    public float MoveToBackDuration { get; private set; } = 0.7f;

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnMove { get; private set; } = true;

    [Title("Télégraphe")]
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone rectangulaire")]
    public GameObject RectangularDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Largeur de la charge")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneWidth { get; private set; } = 3.5f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    [field: SuffixLabel("secondes")]
    public float SpawnDuration { get; private set; } = 0.2f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de visée")]
    [field: SuffixLabel("secondes")]
    public float FillDuration { get; private set; } = 1.2f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Verrouillage avant la charge")]
    [field: SuffixLabel("secondes")]
    [field: ValidateInput(nameof(LockBeforeChargeIsValid), "Le verrouillage doit être inférieur ou égal à la durée totale du télégraphe.")]
    public float LockBeforeCharge { get; private set; } = 0.3f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la visée")]
    [field: Tooltip("Une valeur élevée rend le suivi du joueur moins précis.")]
    public float RotationDampening { get; private set; } = 0.1f;

    [Title("Charge et récupération")]
    [field: SerializeField]
    [field: MinValue(0.01f)]
    [field: LabelText("Durée de la charge")]
    [field: SuffixLabel("secondes")]
    public float ChargeDuration { get; private set; } = 0.35f;

    [field: SerializeField]
    [field: LabelText("After-image pendant la charge")]
    public bool TriggerAfterImageOnCharge { get; private set; } = true;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Temps d'impact")]
    [field: SuffixLabel("secondes")]
    public float ImpactDuration { get; private set; } = 0.45f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Temps de respiration")]
    [field: Tooltip("Fenêtre laissée au joueur après l'impact contre le mur.")]
    [field: SuffixLabel("secondes")]
    public float RecoveryDuration { get; private set; } = 1.3f;

    [Title("Animations")]
    [field: SerializeField]
    [field: LabelText("Déplacement vers le fond")]
    public string MoveAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Préparation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Charge")]
    public string ChargeAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Impact")]
    public string ImpactAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Récupération")]
    public string RecoveryAnimation { get; private set; }

    private bool LockBeforeChargeIsValid =>
        LockBeforeCharge >= 0.0f && LockBeforeCharge <= SpawnDuration + FillDuration;
}
