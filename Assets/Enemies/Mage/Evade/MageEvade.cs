using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class MageEvade : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private CircleDamageZone circleDamageZonePrefab;

    public void StartBehaviour(EnemyController enemy)
    {
        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 targetPosition = playerPosition + (currentPosition - playerPosition).normalized * 0.5f;

        Vector3 evadePosition = targetPosition.magnitude <= 0.01f ? Vector3.forward * 7.0f : (targetPosition * -1.0f).normalized * 7.0f;

        Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(targetPosition))
            .Group(Tween.Position(enemy.transform, targetPosition, 0.5f, Ease.InOutCubic))
            .ChainDelay(0.1f)
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = evadePosition)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack))
            .ChainDelay(1.5f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private void SpawnDamageZone(Vector3 position)
    {
        CircleDamageZone circle = Instantiate(circleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        circle.Setup();
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
