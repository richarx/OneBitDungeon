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
    public List<float> RadiusOptions { get; private set; } = new List<float> { 0.12f, 0.15f, 0.18f };

    [Title("Premier tour")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    public float SpawnDuration { get; private set; } = 0.35f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage")]
    public float FillDuration { get; private set; } = 1.1f;

    [field: SerializeField]
    [field: MinValue(0.05f)]
    [field: LabelText("Durée du tour")]
    public float SpinDuration { get; private set; } = 0.9f;

    [field: SerializeField]
    [field: LabelText("Rotation du visuel")]
    public float SpinDegrees { get; private set; } = 720.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Hauteur de la zone")]
    public float ZoneHeight { get; private set; } = 0.06f;

    [Title("Second souffle")]
    [field: SerializeField]
    [field: LabelText("Activer le second souffle")]
    public bool EnableSecondWind { get; private set; }

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: PropertyRange(0.0f, 1.0f)]
    [field: LabelText("Chance de repartir")]
    public float SecondWindChance { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.0f)]
    [field: LabelText("Fausse fatigue")]
    public float SecondWindPauseDuration { get; private set; } = 0.5f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition V2")]
    public float SecondSpawnDuration { get; private set; } = 0.2f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.0f)]
    [field: LabelText("Télégraphe du second tour")]
    public float SecondFillDuration { get; private set; } = 0.55f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.05f)]
    [field: LabelText("Durée du second tour")]
    public float SecondSpinDuration { get; private set; } = 0.65f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: LabelText("Rotation V2")]
    public float SecondSpinDegrees { get; private set; } = -540.0f;

    [field: SerializeField]
    [field: ShowIf(nameof(EnableSecondWind))]
    [field: MinValue(0.1f)]
    [field: LabelText("Multiplicateur de rayon V2")]
    public float SecondRadiusMultiplier { get; private set; } = 1.0f;

    [Title("Récupération")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Fatigue finale")]
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
