using System;
using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageSpawn : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private float radius;

    [Space]
    [SerializeField] private CircleDamageZone circleDamageZonePrefab;

    private Sequence blastSequence;
    private Sequence spawnSequence;

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        enemy.Sprite.transform.position = Vector3.up * 30.0f;
        enemy.shadowSprite.transform.localScale = Vector3.zero;
        enemy.DeactivateHitbox();
        enemy.animator.Play("Ball");

        blastSequence = Sequence.Create()
            .ChainDelay(7.45f)
            .ChainCallback(() =>
            {
                if (enemy.IsExecutionActive(execution))
                    enemy.animator.Play("Blast");
            });

        spawnSequence = Sequence.Create()
            .ChainDelay(2.0f)
            .ChainCallback(() => SpawnDamageZone(enemy.transform.position))
            .ChainDelay(5.0f)
            .Chain(Tween.Alpha(enemy.shadowSprite, 1.0f, 1.0f))
            .Group(Tween.Scale(enemy.shadowSprite.transform, new Vector3(0.1f, 0.1f, 1.0f), Vector3.one, 0.5f))
            .Group(Tween.LocalPositionY(enemy.Sprite.transform, 30.0f, 0.0f, 0.5f, Ease.OutBounce))
            .ChainCallback(() => enemy.ActivateHitbox())
            .ChainDelay(0.5f)
            .ChainCallback(() => execution.Complete());
    }

    private void SpawnDamageZone(Vector3 position)
    {
        CircleDamageZone circleDamageZone = Instantiate(circleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        circleDamageZone.Setup(radius, 3.0f, 2.5f);
    }

    public void StopBehaviour(EnemyController enemy)
    {
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
    }

    public bool isSubBehaviour;
    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        if (blastSequence.isAlive)
            blastSequence.Stop();

        if (spawnSequence.isAlive)
            spawnSequence.Stop();
    }
}
