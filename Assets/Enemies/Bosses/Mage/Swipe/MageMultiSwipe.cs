using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageMultiSwipe : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private GameObject verticalSwipeObject;
    [SerializeField] private GameObject HorizontalSwipeObject;

    private IEnemyBehaviour vertical;
    private IEnemyBehaviour horizontal;

    private Sequence currentSequence;
    private Sequence moveSequence;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage MULTI SWIPE");

        if (vertical == null)
            SetupBehaviours();

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
            .ChainCallback(() => vertical.StartBehaviour(enemy))
            .ChainDelay(0.1f)
            .ChainCallback(() => horizontal.StartBehaviour(enemy))
            .ChainDelay(0.5f)
            .ChainCallback(() => enemy.SelectNewBehaviour())
            ;
    }

    private void SetupBehaviours()
    {
        vertical = verticalSwipeObject.GetComponent<IEnemyBehaviour>();
        horizontal = HorizontalSwipeObject.GetComponent<IEnemyBehaviour>();
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

        vertical.CancelBehaviour(enemy);
        horizontal.CancelBehaviour(enemy);
    }

    public void SetSubBehaviourState(bool state)
    {
    }
}
