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

    private Sequence attackSequence;
    private List<BiscottoRacletteDamageZone> spawnedZones = new List<BiscottoRacletteDamageZone>();
    private bool isRotatingSprite;
    private Transform rotatingSprite;
    private Quaternion spriteInitialLocalRotation;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy);

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
            })
            .ChainDelay(data.SpinDuration)
            .ChainCallback(() =>
            {
                PlayAnimation(enemy, data.FatigueAnimation);
            });

        if (useSecondWind)
        {
            attackSequence
                .ChainDelay(data.SecondWindPauseDuration)
                .ChainCallback(() =>
                {
                    PlayAnimation(enemy, data.SecondWindAnimation);
                    SpawnZone(
                        enemy,
                        data.SecondRadius,
                        data.SecondSpawnDuration,
                        data.SecondFillDuration,
                        data.SecondSpinDuration);
                })
                .ChainDelay(data.SecondSpawnDuration + data.SecondFillDuration)
                .ChainCallback(() =>
                {
                    PlayAnimation(enemy, data.SpinAnimation);
                })
                .ChainDelay(data.SecondSpinDuration)
                .ChainCallback(() =>
                {
                    PlayAnimation(enemy, data.FatigueAnimation);
                });
        }

        attackSequence
            .ChainDelay(data.FinalRecoveryDuration)
            .ChainCallback(() => execution.Complete());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (isRotatingSprite && rotatingSprite != null)
            rotatingSprite.Rotate(Vector3.forward, 1440 * Time.deltaTime, Space.Self);

        if (spawnedZones == null || spawnedZones.Count == 0)
            return;

        Vector3 zonePosition = new Vector3(
            enemy.transform.position.x,
            enemy.transform.position.y,
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
        ResetRuntimeState(enemy);
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy);
    }

    public void SetSubBehaviourState(bool state)
    {
    }

    private void SpawnZone(EnemyController enemy, float radius, float zoneSpawnDuration, float zoneFillDuration, float activeDuration)
    {
        Vector3 spawnPosition = new Vector3(
            enemy.transform.position.x,
            enemy.transform.position.y,
            enemy.transform.position.z);

        BiscottoRacletteDamageZone zone = UnityEngine.Object.Instantiate(
            data.DamageZonePrefab,
            spawnPosition,
            Quaternion.Euler(90.0f, 0.0f, 0.0f));

        zone.Setup(radius, zoneSpawnDuration, zoneFillDuration, activeDuration, data.HitStaggerPower);
        spawnedZones.Add(zone);
    }

    private float SelectRadius()
    {
        if (data.RadiusOptions == null || data.RadiusOptions.Count == 0)
            return 0.15f;

        return Mathf.Max(0.0f, data.RadiusOptions[UnityEngine.Random.Range(0, data.RadiusOptions.Count)]);
    }

    private void ResetRuntimeState(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (spawnedZones == null)
        {
            spawnedZones = new List<BiscottoRacletteDamageZone>();
        }
        else
        {
            foreach (BiscottoRacletteDamageZone zone in spawnedZones)
            {
                if (zone != null && !zone.IsDestroyed)
                    zone.Cancel();
            }
        }

        attackSequence = default;
        spawnedZones.Clear();
    }

    private static void PlayAnimation(EnemyController enemy, string animationName)
    {
        if (enemy.animator != null && !string.IsNullOrWhiteSpace(animationName))
            enemy.animator.Play(animationName);
    }
}
