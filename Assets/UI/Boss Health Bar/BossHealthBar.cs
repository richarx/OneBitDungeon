using System;
using Enemies.Scripts;
using PrimeTween;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;

    [Space]
    [SerializeField] private RectTransform orbHolder;
    [SerializeField] private GameObject orbPrefab;

    [Space]
    [SerializeField] private Image redHealthbar;
    [SerializeField] private Image yellowHealthbar;

    private Damageable bossDamageable;

    private Sequence updateHealthSequence;
    private Sequence resetHealthSequence;

    private void Start()
    {
        Assert.IsNotNull(enemyController, "EnemyController is not set in Boss Healthbar");

        bossDamageable = enemyController.GetComponent<Damageable>();
        bossDamageable.OnTakeDamage.AddListener((direction) => UpdateHealthBar());
        bossDamageable.OnDie.AddListener(() => UpdateHealthBar());
        bossDamageable.OnResetHealth.AddListener(() => ResetHealthBar());

        SetupOrbs();
    }

    private void UpdateHealthBar()
    {
        if (resetHealthSequence.isAlive)
            resetHealthSequence.Complete();

        if (updateHealthSequence.isAlive)
            updateHealthSequence.Stop();

        updateHealthSequence = ComputeUpdateHealthBarSequence(bossDamageable.currentHealthNormalized);
    }

    private Sequence ComputeUpdateHealthBarSequence(float healthTarget)
    {
        return Sequence.Create()
            .Chain(Tween.UIFillAmount(redHealthbar, healthTarget, 0.2f, Ease.OutQuad))
            .ChainDelay(0.3f)
            .Chain(Tween.UIFillAmount(yellowHealthbar, healthTarget, 0.2f, Ease.OutQuad));
    }

    private void ResetHealthBar()
    {
        if (updateHealthSequence.isAlive)
            updateHealthSequence.Stop();

        if (resetHealthSequence.isAlive)
            resetHealthSequence.Stop();

        resetHealthSequence = ComputeUpdateHealthBarSequence(0.0f);

        resetHealthSequence
            .Chain(Tween.UIFillAmount(redHealthbar, bossDamageable.currentHealthNormalized, 0.5f, Ease.OutQuad))
            .ChainCallback(() => yellowHealthbar.fillAmount = bossDamageable.currentHealthNormalized);

        if (enemyController.currentPhase > 0)
        {
            Image currentOrb = orbHolder.GetChild(enemyController.currentPhase - 1).GetChild(0).GetComponent<Image>();
            Sequence.Create()
                .ChainDelay(0.7f)
                .Chain(Tween.UIFillAmount(currentOrb, 0.0f, 0.5f, Ease.OutQuad));
        }
    }

    private void SetupOrbs()
    {
        for (int i = 0; i < enemyController.Phases.Count - 1; i++)
        {
            Instantiate(orbPrefab, orbHolder);
        }
    }
}
