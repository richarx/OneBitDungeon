using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoEndlessPunchData", menuName = "ScriptableObjects/Biscotto/Endless Punch Data")]
public sealed class BiscottoEndlessPunchData : ScriptableObject
{
    [Title("Mise en place")]
    [field: SerializeField]
    [field: LabelText("Position de Biscotto au fond")]
    [field: Tooltip("Coordonnées X/Z où Biscotto se place avant de commencer la rafale.")]
    public Vector2 BackPosition { get; private set; } = new Vector2(0.0f, 8.0f);

    [field: SerializeField]
    [field: LabelText("Repère avant pour la projection")]
    [field: Tooltip("La direction allant de la position de Biscotto vers ce repère définit la projection du joueur.")]
    public Vector2 FrontPosition { get; private set; } = new Vector2(0.0f, -8.0f);

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Puissance de projection du joueur")]
    public float PlayerPushPower { get; private set; } = 35.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée vers le fond")]
    [field: SuffixLabel("secondes")]
    public float MoveToBackDuration { get; private set; } = 0.7f;

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnMove { get; private set; } = true;

    [Title("Punchs")]
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone rectangulaire")]
    public GameObject RectangularDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Intervalle entre deux punchs")]
    [field: SuffixLabel("secondes")]
    public float PunchInterval { get; private set; } = 0.4f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Largeur de la zone")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneWidth { get; private set; } = 3.0f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Longueur de la zone")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneLength { get; private set; } = 10.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Offset latéral de la zone")]
    [field: Tooltip("Les punchs alternent de part et d'autre de Biscotto.")]
    [field: SuffixLabel("mètres")]
    public float DamageZoneSideOffset { get; private set; } = 2.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Décalage du couloir d'esquive")]
    [field: Tooltip("Décale alternativement la ligne dangereuse autour du joueur. Le côté opposé au décalage devient la sortie la plus courte. Une valeur de 0 conserve une visée centrée.")]
    [field: SuffixLabel("mètres")]
    [field: ValidateInput(nameof(DodgeCorridorOffsetIsValid), "Le décalage doit rester inférieur ou égal à la moitié de la largeur pour que le joueur commence dans le couloir.")]
    public float DodgeCorridorOffset { get; private set; } = 0.8f;

    [field: SerializeField]
    [field: LabelText("Côté du premier punch")]
    public BiscottoPunchSide FirstPunchSide { get; private set; } = BiscottoPunchSide.Left;

    [Title("Télégraphe")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    [field: SuffixLabel("secondes")]
    public float SpawnDuration { get; private set; } = 0.3f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage")]
    [field: SuffixLabel("secondes")]
    public float FillDuration { get; private set; } = 0.6f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Verrouillage avant impact")]
    [field: SuffixLabel("secondes")]
    [field: ValidateInput(nameof(LockBeforeImpactIsValid), "Le verrouillage doit être inférieur ou égal à la durée totale du télégraphe.")]
    public float LockBeforeImpact { get; private set; } = 0.4f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la visée")]
    public float RotationDampening { get; private set; } = 0.4f;

    [Title("Fin après une touche")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération après la touche")]
    [field: SuffixLabel("secondes")]
    public float HitRecoveryDuration { get; private set; } = 0.8f;

    [Title("Animations")]
    [field: SerializeField]
    [field: LabelText("Déplacement vers le fond")]
    public string MoveAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Punch")]
    public string PunchAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Touché par le joueur")]
    public string HitAnimation { get; private set; }

    private bool LockBeforeImpactIsValid =>
        LockBeforeImpact >= 0.0f && LockBeforeImpact <= SpawnDuration + FillDuration;

    private bool DodgeCorridorOffsetIsValid =>
        DodgeCorridorOffset >= 0.0f && DodgeCorridorOffset <= DamageZoneWidth * 0.5f;
}
