using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageRain : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private ThreeCirclesDamageZone threeCirclesDamageZone;

    private bool isSubBehaviour;

    private ThreeCirclesDamageZone circles;
    private Sequence attackSequence;
    private Sequence moveSequence;

    public void StartBehaviour(EnemyController enemy)
    {
        Vector3 randomPosition = Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;

        if (!isSubBehaviour)
        {
            float moveDuration = enemy.isSecondPhase ? 0.5f : 1.0f;

            moveSequence = Sequence.Create()
                .ChainCallback(() => enemy.animator.Play("Cast"))
                .ChainDelay(0.5f)
                .ChainCallback(() =>
                {
                    if (enemy.isSecondPhase)
                        enemy.afterImage.Trigger(moveDuration);
                })
                .Chain(Tween.Position(enemy.transform, randomPosition, moveDuration, Ease.InOutCubic));
        }

        SpawnDamageZone();

        attackSequence = Sequence.Create()
            .ChainCallback(() => SetupCircle(0))
            .ChainDelay(0.5f)
            .ChainCallback(() => SetupCircle(1))
            .ChainDelay(0.5f)
            .ChainCallback(() => SetupCircle(2))
            .ChainDelay(0.5f)
            .ChainCallback(() => circles.Detonate())
            .ChainDelay(enemy.isSecondPhase ? 0.0f : 0.5f)
            .ChainCallback(() =>
            {
                if (!isSubBehaviour)
                    enemy.SelectNewBehaviour();
            });
    }

    private void SpawnDamageZone()
    {
        circles = Instantiate(threeCirclesDamageZone, Vector3.zero, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
    }

    private void SetupCircle(int index)
    {
        circles.Setup(index);
        circles.MoveCircle(index, PlayerStateMachine.instance.position.ToVector2());
    }

    public void UpdateBehaviour(EnemyController enemy)
    {

    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();

        if (moveSequence.isAlive)
            moveSequence.Stop();

        if (circles != null && !circles.hasDetonated)
            circles.Cancel();
    }

    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
