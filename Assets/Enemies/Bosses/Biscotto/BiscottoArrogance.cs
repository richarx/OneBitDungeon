using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BiscottoArrogance : MonoBehaviour
{
    [SerializeField]
    [MinValue(0.01f)]
    [LabelText("Arrogance maximale")]
    private float maxArrogance = 100.0f;

    [ShowInInspector]
    [ReadOnly]
    [LabelText("Arrogance actuelle")]
    private float CurrentArroganceInInspector => currentArrogance;

    [ShowInInspector]
    [ReadOnly]
    [ProgressBar(0.0f, 1.0f)]
    [LabelText("Remplissage")]
    private float NormalizedArroganceInInspector => NormalizedArrogance;

    private float currentArrogance;

    public float CurrentArrogance => currentArrogance;
    public float NormalizedArrogance => maxArrogance <= 0.0f ? 0.0f : currentArrogance / maxArrogance;
    public bool IsFull => maxArrogance > 0.0f && currentArrogance >= maxArrogance;

    public void AddArrogance(float amount)
    {
        currentArrogance = Mathf.Clamp(currentArrogance + Mathf.Max(0.0f, amount), 0.0f, maxArrogance);
    }

    public bool ConsumeFullArrogance()
    {
        if (!IsFull)
            return false;

        currentArrogance = 0.0f;
        return true;
    }
}
