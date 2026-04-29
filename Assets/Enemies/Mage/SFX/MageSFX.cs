using System;
using System.Collections.Generic;
using SFX;
using UnityEngine;

public class MageSFX : MonoBehaviour
{
    [SerializeField] private List<AudioClip> rockMovement;
    [SerializeField] private List<AudioClip> rockBreak;
    [SerializeField] private List<AudioClip> rockThrow;
    [SerializeField] private List<AudioClip> mageMovement;
    [SerializeField] private List<AudioClip> mageFastMovement;

    public static MageSFX instance;

    private EnemyController enemyController;

    private bool isSecondPhase => enemyController.isSecondPhase;

    private void Awake()
    {
        instance = this;
        enemyController = GetComponent<EnemyController>();
    }

    public void PlayRockMove()
    {
        SFXManager.instance.PlayRandomSFX(rockMovement);
    }

    private float lastRockBreakTimestamp = -1.0f;
    public void PlayRockBreak()
    {
        if (Time.time - lastRockBreakTimestamp >= 0.3f)
        {
            SFXManager.instance.PlayRandomSFX(rockBreak, 0.05f);
            lastRockBreakTimestamp = Time.time;
        }
    }

    private float lastRockThrowTimestamp = -1.0f;
    public void PlayRockThrow()
    {
        if (Time.time - lastRockThrowTimestamp >= 0.3f)
        {
            SFXManager.instance.PlayRandomSFX(rockThrow, 0.2f);
            lastRockThrowTimestamp = Time.time;
        }
    }

    public void PlayMageMove()
    {
        SFXManager.instance.PlayRandomSFX(isSecondPhase ? mageFastMovement : mageMovement, isSecondPhase ? 0.3f : 0.05f);
    }
}
