using System;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class BiscottoCrazyRacletteBehaviour : IEnemyBehaviour
{
    [OdinSerialize]
    [Required]
    [LabelText("Prefab de zone Raclette")]
    private BiscottoRacletteDamageZone damageZonePrefab;

    [OdinSerialize]
    [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [LabelText("Rayons possibles")]
    private List<float> radiusOptions = new List<float> { 0.12f, 0.15f, 0.18f };

    [Title("Premier tour")]
    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée d'apparition")]
    private float spawnDuration = 0.35f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Durée de remplissage")]
    private float fillDuration = 1.1f;

    [OdinSerialize]
    [MinValue(0.05f)]
    [LabelText("Durée du tour")]
    private float spinDuration = 0.9f;

    [OdinSerialize]
    [LabelText("Rotation du visuel")]
    private float spinDegrees = 720.0f;

    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Hauteur de la zone")]
    private float zoneHeight = 0.06f;

    [Title("Second souffle")]
    [OdinSerialize]
    [LabelText("Activer le second souffle")]
    private bool enableSecondWind;

    [OdinSerialize]
    [ShowIf(nameof(enableSecondWind))]
    [PropertyRange(0.0f, 1.0f)]
    [LabelText("Chance de repartir")]
    private float secondWindChance = 1.0f;

    [OdinSerialize]
    [ShowIf(nameof(enableSecondWind))]
    [MinValue(0.0f)]
    [LabelText("Fausse fatigue")]
    private float secondWindPauseDuration = 0.5f;

    [OdinSerialize]
    [ShowIf(nameof(enableSecondWind))]
    [MinValue(0.0f)]
    [LabelText("Durée d'apparition V2")]
    private float secondSpawnDuration = 0.2f;

    [OdinSerialize]
    [ShowIf(nameof(enableSecondWind))]
    [MinValue(0.0f)]
    [LabelText("Télégraphe du second tour")]
    private float secondFillDuration = 0.55f;

    [OdinSerialize]
    [ShowIf(nameof(enableSecondWind))]
    [MinValue(0.05f)]
    [LabelText("Durée du second tour")]
    private float secondSpinDuration = 0.65f;

    [OdinSerialize]
    [ShowIf(nameof(enableSecondWind))]
    [LabelText("Rotation V2")]
    private float secondSpinDegrees = -540.0f;

    [OdinSerialize]
    [ShowIf(nameof(enableSecondWind))]
    [MinValue(0.1f)]
    [LabelText("Multiplicateur de rayon V2")]
    private float secondRadiusMultiplier = 1.0f;

    [Title("Récupération")]
    [OdinSerialize]
    [MinValue(0.0f)]
    [LabelText("Fatigue finale")]
    private float finalRecoveryDuration = 1.1f;

    [Title("Animations")]
    [OdinSerialize]
    [LabelText("Préparation")]
    private string anticipationAnimation;

    [OdinSerialize]
    [LabelText("Tourbillon")]
    private string spinAnimation;

    [OdinSerialize]
    [LabelText("Fatigue")]
    private string fatigueAnimation;

    [OdinSerialize]
    [ShowIf(nameof(enableSecondWind))]
    [LabelText("Regain d'énergie")]
    private string secondWindAnimation;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private Sequence spinSequence;
    [NonSerialized] private readonly List<BiscottoRacletteDamageZone> spawnedZones = new List<BiscottoRacletteDamageZone>();
    [NonSerialized] private Quaternion startingRotation;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy, false);
        startingRotation = enemy.transform.rotation;

        if (damageZonePrefab == null)
        {
            Debug.LogError("[BiscottoCrazyRacletteBehaviour] Un prefab de zone Raclette est requis.", enemy);
            execution.Complete();
            return;
        }

        float radius = SelectRadius();
        bool useSecondWind = enableSecondWind && UnityEngine.Random.value <= secondWindChance;

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, anticipationAnimation);
                SpawnZone(enemy, radius, spawnDuration, fillDuration, spinDuration);
            })
            .ChainDelay(spawnDuration + fillDuration)
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, spinAnimation);
                StartSpin(enemy, spinDuration, spinDegrees);
            })
            .ChainDelay(spinDuration)
            .ChainCallback(() => PlayAnimation(enemy, fatigueAnimation));

        if (useSecondWind)
        {
            attackSequence
                .ChainDelay(secondWindPauseDuration)
                .ChainCallback(() =>
                {
                    PlayAnimation(enemy, secondWindAnimation);
                    SpawnZone(
                        enemy,
                        radius * secondRadiusMultiplier,
                        secondSpawnDuration,
                        secondFillDuration,
                        secondSpinDuration);
                })
                .ChainDelay(secondSpawnDuration + secondFillDuration)
                .ChainCallback(() =>
                {
                    PlayAnimation(enemy, spinAnimation);
                    StartSpin(enemy, secondSpinDuration, secondSpinDegrees);
                })
                .ChainDelay(secondSpinDuration)
                .ChainCallback(() => PlayAnimation(enemy, fatigueAnimation));
        }

        attackSequence
            .ChainDelay(finalRecoveryDuration)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        Vector3 zonePosition = new Vector3(
            enemy.transform.position.x,
            enemy.transform.position.y + zoneHeight,
            enemy.transform.position.z);

        for (int i = spawnedZones.Count - 1; i >= 0; i--)
        {
            BiscottoRacletteDamageZone zone = spawnedZones[i];
            if (zone == null || zone.IsDestroyed)
            {
                spawnedZones.RemoveAt(i);
                continue;
            }

            zone.transform.position = zonePosition;
        }
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy, true);
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy, true);
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void SpawnZone(EnemyController enemy, float radius, float zoneSpawnDuration, float zoneFillDuration, float activeDuration)
    {
        Vector3 spawnPosition = new Vector3(
            enemy.transform.position.x,
            enemy.transform.position.y + zoneHeight,
            enemy.transform.position.z);

        BiscottoRacletteDamageZone zone = UnityEngine.Object.Instantiate(
            damageZonePrefab,
            spawnPosition,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));

        zone.Setup(radius, zoneSpawnDuration, zoneFillDuration, activeDuration);
        spawnedZones.Add(zone);
    }

    private void StartSpin(EnemyController enemy, float duration, float degrees)
    {
        if (spinSequence.isAlive)
            spinSequence.Stop();

        Quaternion spinStartRotation = enemy.transform.rotation;
        spinSequence = Sequence.Create()
            .Group(Tween.Custom(
                enemy.transform,
                0.0f,
                degrees,
                duration,
                (target, angle) => target.rotation = spinStartRotation * Quaternion.Euler(0.0f, angle, 0.0f),
                Ease.Linear));
    }

    private float SelectRadius()
    {
        if (radiusOptions == null || radiusOptions.Count == 0)
            return 0.15f;

        return Mathf.Max(0.0f, radiusOptions[UnityEngine.Random.Range(0, radiusOptions.Count)]);
    }

    private void ResetRuntimeState(EnemyController enemy, bool restoreRotation)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (spinSequence.isAlive)
            spinSequence.Stop();

        foreach (BiscottoRacletteDamageZone zone in spawnedZones)
        {
            if (zone != null && !zone.IsDestroyed)
                zone.Cancel();
        }

        if (restoreRotation && enemy != null)
            enemy.transform.rotation = startingRotation;

        attackSequence = default;
        spinSequence = default;
        spawnedZones.Clear();
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
