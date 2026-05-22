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
}
