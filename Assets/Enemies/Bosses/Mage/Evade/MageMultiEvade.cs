using System;
using Enemies.Scripts.Behaviours;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class MageMultiEvade : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private float radius;

    [Space]
    [SerializeField] private MageEvadeSpell mageEvadeSpellPrefab;
    [SerializeField] private HollowCircleDamageZone hollowCircleDamageZonePrefab;
    [SerializeField] private MageData mageData;

    private Sequence attackSequence;

    public void StartBehaviour(EnemyController enemy)
    {
        Debug.Log("Mage MULTI EVADE");

        Vector3 currentPosition = enemy.transform.position;
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        Vector3 targetPosition = playerPosition + (currentPosition - playerPosition).normalized * 0.5f;

        Vector3 evadePosition = targetPosition.magnitude <= 0.01f ? Vector3.forward * 7.0f : (targetPosition * -1.0f).normalized * 7.0f;
        Vector3 evadePosition_2 = ComputeRandomPosition();
        Vector3 evadePosition_3 = ComputeRandomPosition();

        attackSequence = Sequence.Create()
            .ChainCallback(() => SpawnDamageZone(targetPosition))
            .Chain(MoveMageToPosition(enemy, targetPosition))
            .Chain(TeleportMageToPosition(enemy, evadePosition))

            .ChainCallback(() => SpawnDamageZone(evadePosition))
            .Chain(MoveMageToPosition(enemy, evadePosition))
            .Chain(TeleportMageToPosition(enemy, evadePosition_2))

            .ChainCallback(() => SpawnDamageZone(evadePosition_2, () => SpawnHollowCircle(evadePosition_2)))
            .Chain(MoveMageToPosition(enemy, evadePosition_2))
            .Chain(TeleportMageToPosition(enemy, evadePosition_3))
            .ChainDelay(mageData.multiEvadeRecoveryDuration)
            .ChainCallback(() => enemy.SelectNewBehaviour());
    }

    private Sequence MoveMageToPosition(EnemyController enemy, Vector3 enemyPosition)
    {

        return Sequence.Create()
            .ChainCallback(() =>
            {
                enemy.afterImage.Trigger(mageData.multiEvadeSpawnDuration);
                MageSFX.instance.PlayMageMove();
            })
            .Group(Tween.Position(enemy.transform, enemyPosition, mageData.multiEvadeSpawnDuration, Ease.InOutCubic));
    }

    private Sequence TeleportMageToPosition(EnemyController enemy, Vector3 position)
    {
        return Sequence.Create()
           .ChainCallback(() => enemy.animator.Play("Blast"))
            .Chain(Tween.ScaleX(enemy.transform, 0.0f, 0.3f, Ease.InBack))
            .ChainCallback(() => enemy.transform.position = position)
            .Chain(Tween.ScaleX(enemy.transform, 1.0f, 0.3f, Ease.OutBack));
    }

    private Vector3 ComputeRandomPosition()
    {
        float range = 7.5f;
        float x = UnityEngine.Random.Range(-range, range);
        float z = UnityEngine.Random.Range(-range, range);
        return new Vector3(x, 0.0f, z);
    }

    private void SpawnDamageZone(Vector3 position, Action onShootCallback = null)
    {
        MageEvadeSpell spell = Instantiate(mageEvadeSpellPrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        spell.Setup(radius, mageData.multiEvadeSpawnDuration, mageData.multiEvadeFillDuration, onShootCallback);
    }

    private void SpawnHollowCircle(Vector3 position)
    {
        HollowCircleDamageZone hollowCircle = Instantiate(hollowCircleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        hollowCircle.Setup();
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

    public void CancelBehaviour(EnemyController enemy)
    {
        if (attackSequence.isAlive)
            attackSequence.Stop();
    }

    public bool isSubBehaviour;
    public void SetSubBehaviourState(bool state)
    {
        isSubBehaviour = state;
    }
}
