using System;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class AxeController : MonoBehaviour
{
    private DealDamageToPlayer dealDamageToPlayer;

    private bool isSetup;

    public void Setup(Vector3 direction, float distance, float axeMoveDuration, Action callback)
    {
        dealDamageToPlayer = GetComponent<DealDamageToPlayer>();
        isSetup = true;

        direction = direction.normalized;

        Vector3 startingPosition = transform.position;
        Vector3 targetPosition = startingPosition + direction * distance;
        Vector3 endPosition = startingPosition + direction * 2.0f;

        Sequence.Create()
            .Chain(Tween.Position(transform, targetPosition, axeMoveDuration, Ease.OutQuad))
            .Chain(Tween.Position(transform, startingPosition, axeMoveDuration, Ease.InQuad))
            .ChainCallback(() => MakeEnemyCatchAxe(callback));
    }

    private void Update()
    {
        if (!isSetup)
            return;

        CheckForPlayerDamage();
    }

    private void MakeEnemyCatchAxe(Action callback)
    {
        if (callback != null)
            callback();

        Destroy(gameObject);
        isSetup = false;
    }

    private void CheckForPlayerDamage()
    {
        Vector3 directionToPlayer = PlayerStateMachine.instance.position - transform.position;

        if (directionToPlayer.magnitude <= 2.0f)
            dealDamageToPlayer.TryDealDamage(directionToPlayer.normalized);
    }
}
