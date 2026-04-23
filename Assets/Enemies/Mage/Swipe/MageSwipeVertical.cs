using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageSwipeVertical : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private RectangleDamageZone rectangleDamageZonePrefab;

    public void StartBehaviour(EnemyController enemy)
    {
        Vector3 randomPosition = Random.insideUnitSphere * 7.0f;
        randomPosition.y = 0.0f;

        Sequence.Create()
            .ChainDelay(0.5f)
            .Chain(Tween.Position(enemy.transform, randomPosition, 1.0f, Ease.InOutCubic));

        Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(new Vector3(8.92f, 0.0f, 10.0f), Vector2.down))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(4.42f, 0.0f, -10.0f), Vector2.up))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(0.0f, 0.0f, 10.0f), Vector2.down))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(-4.58f, 0.0f, -10.0f), Vector2.up))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(-9.11f, 0.0f, 10.0f), Vector2.down))
            .ChainDelay(2.5f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private void SpawnDamageZone(Vector3 position, Vector2 direction)
    {
        RectangleDamageZone rectangle = Instantiate(rectangleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        rectangle.Setup(direction);
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
