using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class MageRain : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private ThreeCirclesDamageZone threeCirclesDamageZone;

    private ThreeCirclesDamageZone circles;

    public void StartBehaviour(EnemyController enemy)
    {
        Vector3 randomPosition = Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;

        Sequence.Create()
            .ChainDelay(0.5f)
            .Chain(Tween.Position(enemy.transform, randomPosition, 1.0f, Ease.InOutCubic));

        SpawnDamageZone();

        Sequence.Create()
            .ChainCallback(() => SetupCircle(0))
            .ChainDelay(0.5f)
            .ChainCallback(() => SetupCircle(1))
            .ChainDelay(0.5f)
            .ChainCallback(() => SetupCircle(2))
            .ChainCallback(() => circles.Detonate())
            .ChainDelay(0.5f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
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
    }
}
