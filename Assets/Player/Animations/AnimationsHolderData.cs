using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AnimationHolderData")]
public class AnimationsHolderData : ScriptableObject
{
    public AnimationData Idle;
    public AnimationData Walk;
    public AnimationData Jump;
    public AnimationData Roll;
    public AnimationData Attack;
    public AnimationData ParryStart;
    public AnimationData ParrySuccess;
    public AnimationData ParryRecovery;
    public AnimationData Hurt;
    public AnimationData Die;
    public AnimationData GetUp;
    public AnimationData SitDown;
    public AnimationData Sit;
}
