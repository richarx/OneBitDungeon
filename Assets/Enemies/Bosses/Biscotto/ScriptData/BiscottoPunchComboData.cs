using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoPunchComboData", menuName = "ScriptableObjects/Biscotto/Punch Combo Data")]
public sealed class BiscottoPunchComboData : ScriptableObject
{
    [field: SerializeField]
    [field: LabelText("Nom du pattern")]
    public string PatternName { get; private set; } = "Grosse Patate";

    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone rectangulaire")]
    public GameObject RectangularDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [field: LabelText("Coups")]
    public List<BiscottoPunchStep> PunchSteps { get; private set; } = new List<BiscottoPunchStep>();

    [Title("Impact")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Puissance de projection")]
    public float HitStaggerPower { get; private set; } = 30.0f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la visée")]
    public float RotationDampening { get; private set; } = 0.08f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance minimale de suivi")]
    [field: Tooltip("Dans ce rayon autour de l'origine du coup, le télégraphe conserve sa dernière direction afin d'éviter une rotation brutale.")]
    [field: SuffixLabel("mètres")]
    public float MinimumTrackingDistance { get; private set; } = 1.5f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération finale")]
    [field: SuffixLabel("secondes")]
    public float FinalRecoveryDuration { get; private set; } = 0.8f;

    [SerializeField]
    [LabelText("Interrompre le combo si un coup touche")]
    [Tooltip("Dès la première touche, annule les coups restants et joue une animation de touche.")]
    private bool _interruptComboOnHit;

    public bool InterruptComboOnHit => _interruptComboOnHit;

    [field: SerializeField]
    [field: ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [field: LabelText("Animations si un coup touche")]
    public List<string> HitAnimations { get; private set; } = new List<string>();

    [field: SerializeField]
    [field: LabelText("Backdash")]
    public bool Backdash { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(Backdash))]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance au joueur après le backdash")]
    [field: SuffixLabel("mètres")]
    public float BackdashDistance { get; private set; } = 5.0f;

    [field: SerializeField]
    [field: ShowIf(nameof(Backdash))]
    [field: MinValue(0.001f)]
    [field: LabelText("Distance maximale du centre")]
    [field: Tooltip("Empêche un backdash long d'envoyer Biscotto jusqu'au bord de la salle.")]
    [field: SuffixLabel("mètres")]
    public float BackdashMaximumDistanceFromCenter { get; private set; } = 6.0f;

    [field: SerializeField]
    [field: ShowIf(nameof(Backdash))]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du backdash")]
    [field: SuffixLabel("secondes")]
    public float BackdashDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = true;
}
