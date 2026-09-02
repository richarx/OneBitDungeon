using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorTrapData", menuName = "ScriptableObjects/Gladiator/Trap Data")]
public class GladiatorTrapData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone circulaire")]
    public CircleDamageZone CircleDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de trap")]
    public TrapController TrapPrefab;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Radius des zones de damage")]
    [field: SuffixLabel("mètres")]
    public float ZoneRadius;

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
    [field: LabelText("Lance les traps en ligne")]
    public bool ShootTrapsInLine { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(ShootTrapsInLine))]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance entre les traps")]
    public float DistanceBetweenTraps;

    [field: SerializeField]
    [field: HideIf(nameof(ShootTrapsInLine))]
    [field: LabelText("Lance les traps en cercle")]
    public bool ShootTrapsInCircle { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(ShootTrapsInCircle))]
    [field: LabelText("Lance les traps autour du joueur")]
    public bool ShootTrapsAroundPlayer { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(ShootTrapsInCircle))]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance des traps et du centre du cercle de traps")]
    public float TrapsDistanceCenterOfTrapCircle;

    [field: SerializeField]
    [field: MinValue(1)]
    [field: LabelText("Nombre de traps lancés")]
    public int TrapCount;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Hauteur de lancé des traps")]
    [field: SuffixLabel("secondes")]
    public float FlyStartingHeight;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de vol des traps")]
    [field: SuffixLabel("secondes")]
    public float FlyDuration;

    [field: SerializeField]
    [field: LabelText("S'éloigne du joueur")]
    public bool MoveAwayFromPlayer { get; private set; }

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
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de l'animation de lancé")]
    [field: SuffixLabel("secondes")]
    public float ThrowAnimationDuration;

    [field: SerializeField]
    [field: LabelText("Animation d'impact")]
    public string ImpactAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = true;
}
