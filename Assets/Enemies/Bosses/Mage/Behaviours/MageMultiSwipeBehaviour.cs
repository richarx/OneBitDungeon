using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public sealed class MageMultiSwipeBehaviour : IEnemyBehaviour
{
    [OdinSerialize, Required] private MageSwipeVerticalBehaviour verticalSwipe;
    [OdinSerialize, Required] private MageSwipeHorizontalBehaviour horizontalSwipe;

    [NonSerialized] private Sequence currentSequence;
    [NonSerialized] private Sequence moveSequence;
    [NonSerialized] private CloseDodgeSession closeDodgeSession;
    public MageMultiSwipeBehaviour()
    {
    }

    public MageMultiSwipeBehaviour(
        MageSwipeVerticalBehaviour verticalSwipe,
        MageSwipeHorizontalBehaviour horizontalSwipe)
    {
        this.verticalSwipe = verticalSwipe;
        this.horizontalSwipe = horizontalSwipe;
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        ResetRuntimeState(enemy);
        Debug.Log("Mage MULTI SWIPE");
        closeDodgeSession = new CloseDodgeSession(10);
        verticalSwipe.SetCloseDodgeSession(closeDodgeSession);
        horizontalSwipe.SetCloseDodgeSession(closeDodgeSession);
        verticalSwipe.SetSubBehaviourState(true);
        horizontalSwipe.SetSubBehaviourState(true);
        Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;
        const float duration = 0.4f;

        moveSequence = Sequence.Create()
            .ChainCallback(() => enemy.animator.Play("Cast"))
            .ChainDelay(0.5f)
            .ChainCallback(() =>
            {
                enemy.afterImage.Trigger(duration);
                MageSFX.instance.PlayMageMove();
            })
            .Chain(Tween.Position(enemy.transform, randomPosition, duration, Ease.InOutCubic));

        currentSequence = Sequence.Create()
            .ChainCallback(() => verticalSwipe.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(0.1f)
            .ChainCallback(() => horizontalSwipe.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(1.55f)
            .ChainCallback(() => execution.Complete());
    }
    public void UpdateBehaviour(EnemyController enemy) { }
    public void FixedUpdateBehaviour(EnemyController enemy) { }
    public void StopBehaviour(EnemyController enemy)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        ResetRuntimeState(enemy);
    }

    public void SetSubBehaviourState(bool state) { }

    private void ResetRuntimeState(EnemyController enemy)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();

        verticalSwipe?.CancelBehaviour(enemy);
        horizontalSwipe?.CancelBehaviour(enemy);
        closeDodgeSession?.Cancel();
        currentSequence = default;
        moveSequence = default;
        closeDodgeSession = null;
    }
}
