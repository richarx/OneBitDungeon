using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class GladiatorThrowAxe : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private GladiatorData gladiatorData;
    [SerializeField] private GameObject rectangleDamageZonePrefab;
    [SerializeField] private AxeController axePrefab;

    private Sequence attackSequence;
    private RectangleDamageZone rectangleDamageZone;
    private float startAimingTimestamp;
    public Vector3 rotationDirection { get; private set; }

    public void StartBehaviour(EnemyController enemy)
    {
        Vector3 randomPosition = new Vector3(Random.Range(-7.0f, 7.0f), 0.0f, 7.0f);
        string direction = (randomPosition.x - enemy.transform.position.x) >= 0.0f ? "R" : "L";

        attackSequence = Sequence.Create()
                    .ChainCallback(() => enemy.animator.Play($"Dash_{direction}_Axe"))
                    .Chain(MoveToPosition(enemy, randomPosition, gladiatorData.throwMoveDuration))
                    .ChainCallback(() => enemy.animator.Play("ThrowAxe_Anticipation"))
                    .ChainCallback(() => SpawnRectangleZone(enemy))
                    .ChainDelay(gladiatorData.throwSpawnDuration + gladiatorData.throwFillDuration - gladiatorData.throwAnimationDuration)
                    .ChainCallback(() => enemy.animator.Play("ThrowAxe"))
                    .ChainDelay(gladiatorData.throwAnimationDuration)
                    .ChainCallback(() => SpawnAxe(enemy));
    }

    private void SpawnRectangleZone(EnemyController enemy)
    {
        startAimingTimestamp = Time.time;
        GameObject rectangle = Instantiate(rectangleDamageZonePrefab, enemy.transform.position, Quaternion.identity);
        rectangleDamageZone = rectangle.transform.GetChild(0).GetComponent<RectangleDamageZone>();
        rectangleDamageZone.Setup(Vector2.right, gladiatorData.throwSpawnDuration, gladiatorData.throwFillDuration);
    }

    private void SpawnAxe(EnemyController enemy)
    {
        Vector3 position = enemy.transform.position;
        AxeController axe = Instantiate(axePrefab, position, Quaternion.identity);
        axe.Setup(rotationDirection, gladiatorData.throwAxeDistance, gladiatorData.throwAxeFlyDuration, () => CatchAxe(enemy));
    }

    public void CatchAxe(EnemyController enemy)
    {
        attackSequence = Sequence.Create()
            .ChainCallback(() => enemy.animator.Play("CatchAxe"))
            .ChainDelay(1.0f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private Sequence MoveToPosition(EnemyController enemy, Vector3 enemyPosition, float moveDuration)
    {
        bool isSecondPhase = enemy.currentPhase > 0;

        return Sequence.Create()
            .ChainCallback(() =>
            {
                if (isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
            })
            .Group(Tween.Position(enemy.transform, enemyPosition, moveDuration, Ease.InOutCubic));
    }

    public void Update()
    {

    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (rectangleDamageZone != null && Time.time - startAimingTimestamp <= gladiatorData.throwRotationDuration)
            rotationDirection = RotateThrowTowardPlayer();
    }

    private Vector3 RotateThrowTowardPlayer()
    {
        Vector3 position = rectangleDamageZone.transform.parent.position;
        Vector3 direction = (PlayerStateMachine.instance.position - position).normalized;

        rectangleDamageZone.transform.parent.rotation = Quaternion.Slerp(rectangleDamageZone.transform.parent.rotation, Quaternion.LookRotation(direction.ToVector2().AddAngleToDirection(90.0f).ToVector3()), Time.deltaTime / gladiatorData.throwRotationDampening);

        return direction;
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        rectangleDamageZone.Cancel();

        if (attackSequence.isAlive)
            attackSequence.Stop();
    }

    public void SetSubBehaviourState(bool state)
    {
    }
}
