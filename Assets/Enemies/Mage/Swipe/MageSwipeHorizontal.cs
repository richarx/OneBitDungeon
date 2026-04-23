using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageSwipeHorizontal : MonoBehaviour, IEnemyBehaviour
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
            .ChainCallback(() => SpawnDamageZone(new Vector3(10.0f, 0.0f, 8.92f), Vector2.left))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(-10.0f, 0.0f, 4.42f), Vector2.right))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(10.0f, 0.0f, 0.0f), Vector2.left))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(-10.0f, 0.0f, -4.58f), Vector2.right))
            .ChainDelay(0.05f)
            .ChainCallback(() => SpawnDamageZone(new Vector3(10.0f, 0.0f, -9.11f), Vector2.left))
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
