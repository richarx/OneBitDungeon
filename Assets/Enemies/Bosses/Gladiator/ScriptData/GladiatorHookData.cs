using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorHookData", menuName = "ScriptableObjects/Gladiator/Hook Data")]
public class GladiatorHookData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone rectangulaire")]
    public GameObject RectangleDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de hook")]
    public HookController HookControllerPrefab;

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
    [field: LabelText("Durée de l'animation de lancé de hook")]
    [field: SuffixLabel("secondes")]
    public float HookThrowAnimationDuration;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance parcourue par le hook")]
    [field: SuffixLabel("mètres")]
    public float FlyDistance;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de vol du hook")]
    [field: SuffixLabel("mètres")]
    public float FlyDuration;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance de pull du joueur on hit")]
    [field: SuffixLabel("mètres")]
    public float PullDistance;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du pull du joueur on hit")]
    [field: SuffixLabel("secondes")]
    public float PullDuration;

    [field: SerializeField]
    [field: LabelText("S'éloigne du joueur")]
    public bool MoveAwayFromPlayer { get; private set; }

    [field: SerializeField]
    [field: HideIf(nameof(MoveAwayFromPlayer))]
    [field: LabelText("Se déplace vers un coin du fond de l'arene")]
    public bool MoveToCornerPosition { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(MoveToCornerPosition))]
    [field: LabelText("Se déplace vers le coin le plus éloigné")]
    public bool GoToOppositeCorner { get; private set; }


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
