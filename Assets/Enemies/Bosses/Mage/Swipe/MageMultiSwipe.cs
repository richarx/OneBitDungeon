using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageMultiSwipe : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private GameObject verticalSwipeObject;
    [SerializeField] private GameObject HorizontalSwipeObject;

    private IEnemyBehaviour vertical;
    private IEnemyBehaviour horizontal;
    private MageSwipeVertical verticalSwipe;
    private MageSwipeHorizontal horizontalSwipe;

    private Sequence currentSequence;
    private Sequence moveSequence;
    private CloseDodgeSession closeDodgeSession;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        Debug.Log("Mage MULTI SWIPE");

        if (vertical == null || horizontal == null)
        {
            if (!SetupBehaviours(enemy))
                return;
        }

        // Five vertical and five horizontal zones form one salvo.
        closeDodgeSession = new CloseDodgeSession(10);
        verticalSwipe.SetCloseDodgeSession(closeDodgeSession);
        horizontalSwipe.SetCloseDodgeSession(closeDodgeSession);

        vertical.SetSubBehaviourState(true);
        horizontal.SetSubBehaviourState(true);

        Vector3 randomPosition = Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;

        float moveDuration = 0.4f;

        moveSequence = Sequence.Create()
        .ChainCallback(() => enemy.animator.Play("Cast"))
        .ChainDelay(0.5f)
        .ChainCallback(() =>
        {
            enemy.afterImage.Trigger(moveDuration);
            MageSFX.instance.PlayMageMove();
        })
        .Chain(Tween.Position(enemy.transform, randomPosition, moveDuration, Ease.InOutCubic));

        currentSequence = Sequence.Create()
            .ChainCallback(() => vertical.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(0.1f)
            .ChainCallback(() => horizontal.StartBehaviour(enemy, BehaviourExecution.Uncontrolled))
            .ChainDelay(1.55f)
            .ChainCallback(() => execution.Complete())
            ;
    }

    private bool SetupBehaviours(EnemyController enemy)
    {
        vertical = enemy.ResolveBehaviour(verticalSwipeObject);
        horizontal = enemy.ResolveBehaviour(HorizontalSwipeObject);

        verticalSwipe = vertical as MageSwipeVertical;
        horizontalSwipe = horizontal as MageSwipeHorizontal;

        if (verticalSwipe == null || horizontalSwipe == null)
        {
            Debug.LogError($"[{enemy.name}] MageMultiSwipe requires MageSwipeVertical and MageSwipeHorizontal behaviour prefabs.", enemy);
            vertical = null;
            horizontal = null;
            return false;
        }

        return true;
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();

        vertical?.CancelBehaviour(enemy);
        horizontal?.CancelBehaviour(enemy);
        closeDodgeSession?.Cancel();
    }

    public void SetSubBehaviourState(bool state)
    {
    }
}
