using System;
using Enemies.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHumility : MonoBehaviour
{
    public static EnemyHumility instance;

    [HideInInspector] public UnityEvent OnUpdateHumility = new UnityEvent();
    [HideInInspector] public UnityEvent OnResetHumility = new UnityEvent();
    public static UnityEvent OnFullHumility = new UnityEvent();

    [SerializeField] private float timeBeforeHumilityReduction;
    [SerializeField] private AnimationCurve timeCurve;

    [SerializeField] private float humilityReductionPower;
    [SerializeField] private AnimationCurve powerCurve;

    public float currentHumility { get; private set; }
    public float maxHumility { get; private set; }

    public float currentHumilityNormalized => Tools.NormalizeValue(currentHumility, 0.0f, maxHumility);

    public bool IsFull => currentHumility >= maxHumility;

    private Damageable damageable;
    private float lastHumilityGainTimestamp;

    private void Awake()
    {
        instance = this;
        damageable = GetComponent<Damageable>();
        damageable.OnTakeDamage.AddListener((direction) => lastHumilityGainTimestamp = Time.time);
    }

    private void Update()
    {
        if (currentHumility > 0 && !IsFull && CheckForHumilityReduction())
            ComputeHumilityReduction();
    }

    private bool CheckForHumilityReduction()
    {
        float curveValue = timeCurve.Evaluate(damageable.currentHealthNormalized);
        float timeSinceLastGain = Time.time - lastHumilityGainTimestamp;

        return timeSinceLastGain >= timeBeforeHumilityReduction * curveValue;
    }

    private void ComputeHumilityReduction()
    {
        float curveValue = powerCurve.Evaluate(damageable.currentHealthNormalized);
        float reduction = humilityReductionPower * curveValue * Time.deltaTime;

        currentHumility = Mathf.Clamp(currentHumility - reduction, 0, maxHumility);
        OnUpdateHumility?.Invoke();
    }

    public void AddHumility(float value)
    {
        currentHumility = Mathf.Clamp(currentHumility + value, 0, maxHumility);
        OnUpdateHumility?.Invoke();
        lastHumilityGainTimestamp = Time.time;

        if (IsFull)
            OnFullHumility?.Invoke();
    }

    public void ResetHumility(float value)
    {
        currentHumility = 0;
        maxHumility = value;
        lastHumilityGainTimestamp = Time.time;
        OnResetHumility?.Invoke();
    }
}
