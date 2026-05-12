using UnityEngine;

[CreateAssetMenu(fileName = "MageData", menuName = "ScriptableObjects/MageData")]
public class MageData : ScriptableObject
{
    [Header("Swipe")]
    public float swipeMoveDuration;
    public float swipeMoveDuration_p2;
    public float swipeSpawnDuration;
    public float swipeFillDuration;
    public float swipeRecoveryDuration;
    public float swipeRecoveryDuration_2;

    [Header("Evade")]
    public float evadeSpawnDuration;
    public float evadeFillDuration;
    public float evadeRecoveryDuration;
    public float evadeRecoveryDuration_p2;

    [Header("Multi-Evade")]
    public float multiEvadeSpawnDuration;
    public float multiEvadeFillDuration;
    public float multiEvadeRecoveryDuration;

    [Header("Throw")]
    public float throwMoveDuration;
    public float throwMoveDuration_p2;
    public float throwSpawnDuration;
    public float throwFillDuration;
    public float throwRecoveryDuration;
    public float throwRecoveryDuration_p2;

    [Header("Multi-Throw")]
    public float multiThrowMoveDuration;
    public float multiThrowMoveDuration_toRight;
    public float multiThrowMoveDuration_toLeft;
    public float multiThrowSpawnDuration;
    public float multiThrowFillDuration;
    public float multiThrowRecoveryDuration;

    [Header("Rain")]
    public float rainMoveDuration;
    public float rainMoveDuration_p2;
    public float rainSpawnDuration;
    public float rainFillDuration;
    public float rainRecoveryDuration;
    public float rainRecoveryDuration_p2;
}
