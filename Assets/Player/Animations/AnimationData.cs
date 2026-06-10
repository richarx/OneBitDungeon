using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AnimationData")]
public class AnimationData : ScriptableObject
{
    public List<Sprite> weaponInTheBackSprites;
    public List<Sprite> weaponInHandSprites;

    [Space]
    public int directionCount;
    public float timeBetweenFrames;

    [Space]
    public bool isLooping;
    public bool canSelfCancel;

    [Space]
    [CanBeNull]
    public AnimationData nextAnimation;

    public List<Sprite> GetSprites(bool hasWeaponInHand)
    {
        return hasWeaponInHand ? weaponInHandSprites : weaponInTheBackSprites;
    }
}
