using UnityEngine;

[CreateAssetMenu(fileName = "MageData", menuName = "ScriptableObjects/MageData")]
public class MageData : ScriptableObject
{
    [Header("Swipe")]
    public float swipeMoveDuration;
    public float swipeSpawnDuration;
    public float swipeFillDuration;
    public float swipeRecoveryDuration;

    [Header("Evade")]
    public float evadeSpawnDuration;
    public float evadeFillDuration;
    public float evadeRecoveryDuration;

    [Header("Throw")]
    public float throwMoveDuration;
    public float throwSpawnDuration;
    public float throwFillDuration;
    public float throwRecoveryDuration;

    [Header("Rain")]
    public float rainMoveDuration;
    public float rainSpawnDuration;
    public float rainFillDuration;
    public float rainRecoveryDuration;
}
