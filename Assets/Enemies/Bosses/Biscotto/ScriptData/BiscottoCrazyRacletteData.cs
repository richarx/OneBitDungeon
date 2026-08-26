using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoCrazyRacletteData", menuName = "ScriptableObjects/Biscotto/Crazy Raclette Data")]
public sealed class BiscottoCrazyRacletteData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone Raclette")]
    public BiscottoRacletteDamageZone DamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [field: LabelText("Rayons possibles")]
    [field: SuffixLabel("mètres")]
    public List<float> RadiusOptions { get; private set; } = new List<float> { 0.12f, 0.15f, 0.18f };

    [Title("Impact")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Puissance de projection")]
    public float HitStaggerPower { get; private set; } = 30.0f;

    [Title("Déplacement")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Vitesse de poursuite")]
    [field: SuffixLabel("mètres / seconde")]
    [field: Tooltip("Vitesse à laquelle Biscotto et sa zone Raclette se déplacent vers le joueur pendant l'attaque.")]
    public float MovementSpeed { get; private set; } = 3.0f;

    [Title("Premier tour")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    [field: SuffixLabel("secondes")]
    public float SpawnDuration { get; private set; } = 0.35f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage")]
    [field: SuffixLabel("secondes")]
    public float FillDuration { get; private set; } = 1.1f;

    [field: SerializeField]
    [field: MinValue(0.05f)]
    [field: LabelText("Durée du tour")]
    [field: SuffixLabel("secondes")]
    public float SpinDuration { get; private set; } = 0.9f;


    [Title("Second souffle")]
    [field: SerializeField]
    [field: LabelText("Activer le second souffle")]
    public bool EnableSecondWind { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: PropertyRange(0.0f, 1.0f)]
    [field: LabelText("Chance de repartir")]
    [field: SuffixLabel("secondes")]
    public float SecondWindChance { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.0f)]
    [field: LabelText("Fausse fatigue")]
    [field: SuffixLabel("secondes")]
    public float SecondWindPauseDuration { get; private set; } = 0.5f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition Second souffle")]
    [field: SuffixLabel("secondes")]
    public float SecondSpawnDuration { get; private set; } = 0.2f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.0f)]
    [field: LabelText("Fill du Second souffle")]
    [field: SuffixLabel("secondes")]
    public float SecondFillDuration { get; private set; } = 0.55f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.05f)]
    [field: LabelText("Durée du second tour")]
    [field: SuffixLabel("secondes")]
    public float SecondSpinDuration { get; private set; } = 0.65f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.0f)]
    [field: LabelText("Rayon du second tour")]
    [field: SuffixLabel("mètres")]
    public float SecondRadius { get; private set; } = 0.15f;

    [Title("Récupération")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Fatigue finale")]
    [field: SuffixLabel("secondes")]
    public float FinalRecoveryDuration { get; private set; } = 1.1f;

    [Title("Animations")]
    [field: SerializeField]
    [field: LabelText("Préparation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Tourbillon")]
    public string SpinAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Fatigue")]
    public string FatigueAnimation { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: LabelText("Regain d'énergie")]
    public string SecondWindAnimation { get; private set; }
}
