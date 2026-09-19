using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHumility : MonoBehaviour
{
    public static EnemyHumility instance;

    [HideInInspector] public UnityEvent OnUpdateHumility = new UnityEvent();
    [HideInInspector] public UnityEvent OnResetHumility = new UnityEvent();

    public int currentHumility { get; private set; }
    public int maxHumility { get; private set; }

    public float currentHumilityNormalized => Tools.NormalizeValue(currentHumility, 0.0f, maxHumility);

    private void Awake()
    {
        instance = this;
    }

    public void AddHumility(int value)
    {
        currentHumility = Mathf.Clamp(currentHumility + value, 0, maxHumility);
        OnUpdateHumility?.Invoke();
    }

    public void ResetHumility(int value)
    {
        currentHumility = 0;
        maxHumility = value;
        OnResetHumility?.Invoke();
    }
}
