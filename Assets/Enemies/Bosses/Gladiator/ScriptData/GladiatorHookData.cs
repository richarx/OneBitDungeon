using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorHookData", menuName = "ScriptableObjects/Gladiator/Hook Data")]
public class GladiatorHookData : ScriptableObject
{
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
}
