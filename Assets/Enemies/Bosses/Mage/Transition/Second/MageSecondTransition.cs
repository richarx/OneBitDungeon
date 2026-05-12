using Enemies.Scripts.Behaviours;
using PrimeTween;
using UnityEngine;

public class MageSecondTransition : MonoBehaviour, IEnemyBehaviour
{
    [SerializeField] private CircleDamageZone circleDamageZonePrefab;
    [SerializeField] private HollowCircleDamageZone hollowCircleDamageZonePrefab;
    [SerializeField] private int maxBounceCount;

    private Sequence bounceSequence;
    private int bounceCount;

    public void StartBehaviour(EnemyController enemy)
    {
        bounceCount = 0;

        bounceSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(enemy.transform, Vector3.zero, 1.0f, Ease.InOutCubic))
            .ChainCallback(() => enemy.DeactivateHitbox())
            .ChainCallback(() => enemy.animator.Play("Charge"))
            .Chain(Tween.LocalPositionY(enemy.sprite.transform, 5.0f, 1.5f, Ease.InOutCubic))
            .Chain(Tween.ShakeScale(enemy.sprite.transform, Vector3.up, 0.5f))
            .ChainCallback(() => enemy.animator.Play("Ball"))
            ;
    }

    private void BounceOnce(EnemyController enemy, Vector3 position)
    {
        bounceSequence = Sequence.Create()
            .ChainCallback(() => SpawnInitialDamageZone(position))
            .Chain(Tween.LocalPosition(enemy.transform, position, 0.3f, Ease.InOutCubic))
            .Chain(Tween.LocalPositionY(enemy.sprite.transform, 8.0f, 0.3f, Ease.OutBack))
            .Chain(Tween.LocalPositionY(enemy.sprite.transform, 0.0f, 0.1f, Ease.OutBack))
            .ChainCallback(() => SpawnHollowCircle(position))
            .Chain(Tween.LocalPositionY(enemy.sprite.transform, 8.0f, 0.5f, Ease.OutBack))
            .Chain(Tween.LocalPosition(enemy.transform, ComputeRandomPosition(), 0.5f, Ease.InOutCubic))
            ;
    }

    private Vector3 ComputeRandomPosition()
    {
        float range = 7.5f;
        float x = Random.Range(-range, range);
        float z = Random.Range(-range, range);
        return new Vector3(x, 0.0f, z);
    }

    private void SpawnInitialDamageZone(Vector3 position)
    {
        CircleDamageZone circleDamageZone = Instantiate(circleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        circleDamageZone.Setup(0.15f, 0.3f, 0.4f);
    }

    private void SpawnHollowCircle(Vector3 position)
    {
        HollowCircleDamageZone hollowCircle = Instantiate(hollowCircleDamageZonePrefab, position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)));
        hollowCircle.Setup();
    }

    public void UpdateBehaviour(EnemyController enemy)
    {
        if (!bounceSequence.isAlive)
        {
            bounceCount += 1;

            if (bounceCount >= maxBounceCount)
                StopTransition(enemy);
            else
                BounceOnce(enemy, ComputeRandomPosition());
        }
    }

    private void StopTransition(EnemyController enemy)
    {
        bounceSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(enemy.transform, Vector3.zero, 1.5f, Ease.InOutCubic))
            .Chain(Tween.LocalPositionY(enemy.sprite.transform, 0.0f, 0.5f, Ease.OutBack))
            .ChainCallback(() => enemy.animator.Play("Blast"))
            .ChainCallback(() => enemy.ActivateHitbox())
            .ChainCallback(() => enemy.SelectNewBehaviour(true))
            ;
    }

    public void FixedUpdateBehaviour(EnemyController enemy)
    {
    }

    public void StopBehaviour(EnemyController enemy)
    {
        enemy.ActivateHitbox();

        if (bounceSequence.isAlive)
            bounceSequence.Stop();

        enemy.sprite.transform.localPosition = Vector3.zero;
    }

    public void CancelBehaviour(EnemyController enemy)
    {
        enemy.ActivateHitbox();

        if (bounceSequence.isAlive)
            bounceSequence.Stop();

        enemy.sprite.transform.localPosition = Vector3.zero;
    }

    public void SetSubBehaviourState(bool state)
    {
    }
}
