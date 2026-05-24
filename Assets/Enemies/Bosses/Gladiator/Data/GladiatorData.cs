using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorData", menuName = "ScriptableObjects/GladiatorData")]
public class GladiatorData : ScriptableObject
{
    [Header("Hook")]
    public float hookMoveDuration;
    public float hookSpawnDuration;
    public float hookFillDuration;
    public float hookRotationDuration;
    public float hookRotationDampening;
    public float hookAnimationDuration;
    public float hookFlyDistance;
    public float hookFlyDuration;
    public float hookPullDistance;
    public float hookPullDuration;

    [Header("Axe Throw")]
    public float throwMoveDuration;
    public float throwSpawnDuration;
    public float throwFillDuration;
    public float throwRotationDuration;
    public float throwRotationDampening;
    public float throwAnimationDuration;
    public float throwAxeDistance;
    public float throwAxeFlyDuration;

    [Header("Traps")]
    public float trapsMoveDuration;
    public float trapsZoneRadius;
    public float trapsSpawnDuration;
    public float trapsFillDuration;
    public float trapsAnimationDuration;
    public float trapsDistanceFromPlayer;
    public float trapsStartingHeight;
    public float trapsFlyDuration;
}
