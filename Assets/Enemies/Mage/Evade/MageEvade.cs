using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class MageEvade : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private CircleDamageZone circleDamageZonePrefab;

    private CircleDamageZone circle;
    private Sequence currentSequence;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage EVADE");

        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 targetPosition = playerPosition + (currentPosition - playerPosition).normalized * 0.5f;

        Vector3 evadePosition = targetPosition.magnitude <= 0.01f ? Vector3.forward * 7.0f : (targetPosition * -1.0f).normalized * 7.0f;

        float moveDuration = enemy.isSecondPhase ? 0.3f : 0.5f;

        currentSequence = Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(targetPosition))
            .ChainCallback(() =>
            {
                if (enemy.isSecondPhase)
                    enemy.afterImage.Trigger(moveDuration);
            })
            .Group(Tween.Position(enemy.transform, targetPosition, moveDuration, Ease.InOutCubic))
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .ChainDelay(0.1f)
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = evadePosition)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack))
            .ChainDelay(enemy.isSecondPhase ? 0.5f : 0.75f)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private void SpawnDamageZone(Vector3 position)
    {
        circle = Instantiate(circleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
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
        if (currentSequence.isAlive)
            currentSequence.Stop();
    }

    public bool isSubBehaviour;
    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
