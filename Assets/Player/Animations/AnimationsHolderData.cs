using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AnimationHolderData")]
public class AnimationsHolderData : ScriptableObject
{
    public AnimationData Idle;
    public AnimationData Walk;
}
