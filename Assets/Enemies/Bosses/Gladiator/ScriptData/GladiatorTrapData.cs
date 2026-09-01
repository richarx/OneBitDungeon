using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorTrapData", menuName = "ScriptableObjects/Gladiator/Trap Data")]
public class GladiatorTrapData : ScriptableObject
{
    public float trapsMoveDuration;
    public float trapsZoneRadius;
    public float trapsSpawnDuration;
    public float trapsFillDuration;
    public float trapsAnimationDuration;
    public float trapsDistanceFromPlayer;
    public float trapsStartingHeight;
    public float trapsFlyDuration;
}
