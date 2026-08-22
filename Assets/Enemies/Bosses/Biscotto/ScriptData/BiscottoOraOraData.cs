using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoOraOraData", menuName = "ScriptableObjects/Biscotto/Ora Ora Data")]
public sealed class BiscottoOraOraData : ScriptableObject
{
    [Title("Coups")]
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone rectangulaire")]
    public GameObject RectangularDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: MinValue(1)]
    [field: LabelText("Nombre de coups")]
    public int PunchCount { get; private set; } = 12;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Intervalle entre deux coups")]
    [field: SuffixLabel("secondes")]
    [field: Tooltip("Les télégraphes déjà lancés continuent pendant que le coup suivant est créé.")]
    public float PunchInterval { get; private set; } = 0.17f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Largeur de la zone")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneWidth { get; private set; } = 3.0f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Longueur de la zone")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneLength { get; private set; } = 5.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Offset latéral de la zone")]
    [field: Tooltip("Distance entre Biscotto et la zone, perpendiculairement à la direction du joueur.")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneSideOffset { get; private set; } = 2.0f;

    [field: SerializeField]
    [field: LabelText("Côté du premier coup")]
    [field: Tooltip("Les coups suivants alternent automatiquement entre gauche et droite.")]
    public BiscottoPunchSide FirstPunchSide { get; private set; } = BiscottoPunchSide.Left;

    [Title("Télégraphe")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    [field: SuffixLabel("secondes")]
    public float SpawnDuration { get; private set; } = 0.15f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage")]
    [field: SuffixLabel("secondes")]
    public float FillDuration { get; private set; } = 0.7f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Verrouillage avant impact")]
    [field: SuffixLabel("secondes")]
    [field: ValidateInput(nameof(LockBeforeImpactIsValid), "Le verrouillage doit être inférieur ou égal à la durée totale du télégraphe.")]
    public float LockBeforeImpact { get; private set; } = 0.15f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la visée")]
    [field: Tooltip("Une valeur élevée rend le suivi du joueur moins précis.")]
    public float RotationDampening { get; private set; } = 0.4f;

    [Title("Repositionnement")]
    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Distance maximale avant déplacement")]
    [field: Tooltip("Biscotto se déplace seulement si le joueur est au-delà de cette distance.")]
    [field: SuffixLabel("mètres")]
    public float RepositionDistanceThreshold { get; private set; } = 6.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance au joueur")]
    [field: SuffixLabel("mètres")]
    public float RepositionDistanceToPlayer { get; private set; } = 1.2f;


    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du repositionnement")]
    [field: SuffixLabel("secondes")]
    public float RepositionDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: LabelText("After-image pendant le repositionnement")]
    public bool TriggerAfterImageOnReposition { get; private set; } = true;

    [Title("Fin")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération finale")]
    [field: SuffixLabel("secondes")]
    public float FinalRecoveryDuration { get; private set; } = 0.8f;

    [field: SerializeField]
    [field: LabelText("Animation de coup")]
    public string PunchAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Animation de récupération")]
    public string RecoveryAnimation { get; private set; }

    private bool LockBeforeImpactIsValid => LockBeforeImpact >= 0.0f && LockBeforeImpact <= SpawnDuration + FillDuration;
}
