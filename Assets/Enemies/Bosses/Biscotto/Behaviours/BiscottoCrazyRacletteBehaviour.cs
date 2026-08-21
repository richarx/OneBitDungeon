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
    [LabelText("Data")]
    private BiscottoCrazyRacletteData data;

    [NonSerialized] private Sequence attackSequence;
    [NonSerialized] private Sequence spinSequence;
    [NonSerialized] private readonly List<BiscottoRacletteDamageZone> spawnedZones = new List<BiscottoRacletteDamageZone>();
    [NonSerialized] private Quaternion startingRotation;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy, false);
        startingRotation = enemy.transform.rotation;

        if (data == null)
        {
            Debug.LogError("[BiscottoCrazyRacletteBehaviour] Un data Crazy Raclette est requis.", enemy);
            execution.Complete();
            return;
        }

        if (data.DamageZonePrefab == null)
        {
            Debug.LogError("[BiscottoCrazyRacletteBehaviour] Un prefab de zone Raclette est requis.", enemy);
            execution.Complete();
            return;
        }

        float radius = SelectRadius();
        bool useSecondWind = data.EnableSecondWind && UnityEngine.Random.value <= data.SecondWindChance;

        attackSequence = Sequence.Create()
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, data.AnticipationAnimation);
                SpawnZone(enemy, radius, data.SpawnDuration, data.FillDuration, data.SpinDuration);
            })
            .ChainDelay(data.SpawnDuration + data.FillDuration)
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, data.SpinAnimation);
                StartSpin(enemy, data.SpinDuration, data.SpinDegrees);
            })
            .ChainDelay(data.SpinDuration)
            .ChainCallback(() => PlayAnimation(enemy, data.FatigueAnimation));

        if (useSecondWind)
        {
            attackSequence
                .ChainDelay(data.SecondWindPauseDuration)
                .ChainCallback(() =>
                {
                    PlayAnimation(enemy, data.SecondWindAnimation);
                    SpawnZone(
                        enemy,
                        radius * data.SecondRadiusMultiplier,
                        data.SecondSpawnDuration,
                        data.SecondFillDuration,
                        data.SecondSpinDuration);
                })
                .ChainDelay(data.SecondSpawnDuration + data.SecondFillDuration)
                .ChainCallback(() =>
                {
                    PlayAnimation(enemy, data.SpinAnimation);
                    StartSpin(enemy, data.SecondSpinDuration, data.SecondSpinDegrees);
                })
                .ChainDelay(data.SecondSpinDuration)
                .ChainCallback(() => PlayAnimation(enemy, data.FatigueAnimation));
        }

        attackSequence
            .ChainDelay(data.FinalRecoveryDuration)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        Vector3 zonePosition = new Vector3(
            enemy.transform.position.x,
            enemy.transform.position.y + data.ZoneHeight,
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
            enemy.transform.position.y + data.ZoneHeight,
            enemy.transform.position.z);

        BiscottoRacletteDamageZone zone = UnityEngine.Object.Instantiate(
            data.DamageZonePrefab,
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
        if (data.RadiusOptions == null || data.RadiusOptions.Count == 0)
            return 0.15f;

        return Mathf.Max(0.0f, data.RadiusOptions[UnityEngine.Random.Range(0, data.RadiusOptions.Count)]);
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
