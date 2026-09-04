using System;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class AxeController : MonoBehaviour
{
    private DealDamageToPlayer dealDamageToPlayer;

    private bool isSetup;
    private bool isComingBack;

    private Sequence moveSequence;

    private Transform bossTransform;
    private Action bossCallback;

    public void Setup(EnemyController enemy, Vector3 direction, float distance, float axeMoveDuration, Action callback)
    {
        dealDamageToPlayer = GetComponent<DealDamageToPlayer>();
        bossTransform = enemy.transform;
        bossCallback = callback;
        isSetup = true;

        direction = direction.normalized;

        Vector3 targetPosition = transform.position + direction * distance;

        moveSequence = Sequence.Create()
            .Chain(Tween.Position(transform, targetPosition, axeMoveDuration, Ease.OutQuad))
            .ChainCallback(() => GoBackToBoss(distance, axeMoveDuration));
    }

    private void GoBackToBoss(float initialDistance, float axeMoveDuration)
    {
        if (moveSequence.isAlive)
            moveSequence.Stop();

        isComingBack = true;

        Vector3 targetPosition = bossTransform.transform.position;
        float targetDistance = (targetPosition - transform.position).magnitude;
        float initialSpeed = initialDistance / axeMoveDuration;

        float targetMoveDuration = targetDistance / initialSpeed;

        moveSequence = Sequence.Create()
           .Chain(Tween.Position(transform, targetPosition, targetMoveDuration, Ease.InQuad))
           .ChainCallback(() => MakeEnemyCatchAxe());
    }

    private void Update()
    {
        if (!isSetup)
            return;

        float distanceFromBoss = (bossTransform.position - transform.position).magnitude;

        if (isComingBack && distanceFromBoss <= 2.0f)
            MakeEnemyCatchAxe();
        else
            CheckForPlayerDamage();
    }

    private void CheckForPlayerDamage()
    {
        Vector3 directionToPlayer = PlayerStateMachine.instance.position - transform.position;

        if (directionToPlayer.magnitude <= 2.0f)
            dealDamageToPlayer.TryDealDamage(directionToPlayer.normalized);
    }

    private void MakeEnemyCatchAxe()
    {
        if (bossCallback != null)
            bossCallback();

        DestroyAxe();
    }

    private void DestroyAxe()
    {
        if (moveSequence.isAlive)
            moveSequence.Stop();

        Destroy(gameObject);
        isSetup = false;
    }
}
