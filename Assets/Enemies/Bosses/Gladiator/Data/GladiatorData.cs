using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorData", menuName = "ScriptableObjects/GladiatorData")]
public class GladiatorData : ScriptableObject
{
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
