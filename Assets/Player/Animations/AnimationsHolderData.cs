using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AnimationHolderData")]
public class AnimationsHolderData : ScriptableObject
{
    public AnimationData Idle;
    public AnimationData ArrogantIdle;
    public AnimationData Walk;
    public AnimationData ArrogantWalk;
    public AnimationData ArrogantSpinLeft;
    public AnimationData ArrogantSpinRight;
    public AnimationData Jump;
    public AnimationData Roll;
    public AnimationData Attack;
    public AnimationData JumpAttack;
    public AnimationData ParryStart;
    public AnimationData ParrySuccess;
    public AnimationData ParryRecovery;
    public AnimationData Hurt;
    public AnimationData Die;
    public AnimationData GetUp;
    public AnimationData SitDown;
    public AnimationData Sit;
}
