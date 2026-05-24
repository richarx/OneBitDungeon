using System;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class HookController : MonoBehaviour
{
    [SerializeField] private Transform hookHead;
    [SerializeField] private LineRenderer lineRenderer;

    private float pullPlayerDistance;
    private float pullPlayerDuration;

    private bool isSetup;
    private bool isTryingToHit = true;

    private Sequence hookSequence;

    public void Setup(Vector3 direction, float distance, float hookFlyDuration, float pullDistance, float pullDuration)
    {
        pullPlayerDistance = pullDistance;
        pullPlayerDuration = pullDuration;
        isSetup = true;

        hookHead.rotation = direction.ToVector2().AddAngleToDirection(90.0f).ToRotation();
        Vector3 targetPosition = direction * distance;

        hookSequence = Sequence.Create()
            .Chain(Tween.LocalPosition(hookHead, targetPosition, hookFlyDuration, Ease.OutQuad))
            .ChainCallback(() => isTryingToHit = false)
            .Chain(Tween.LocalPosition(hookHead, Vector3.zero, hookFlyDuration, Ease.InQuad))
            .ChainCallback(() => DestroyHook());
    }

    private void Update()
    {
        if (!isSetup)
            return;

        UpdateLineRenderer();

        if (isTryingToHit)
            CheckForPlayerHit();
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { hookHead.localPosition, Vector3.zero });
    }

    private void CheckForPlayerHit()
    {
        Vector3 directionToPlayer = PlayerStateMachine.instance.position - hookHead.position;
        directionToPlayer.y = 0.0f;

        bool isInRange = directionToPlayer.magnitude <= 1.5f;
        bool isJumping = PlayerStateMachine.instance.currentBehaviour.GetBehaviourType() == BehaviourType.Jump && !PlayerStateMachine.instance.playerJump.hasLanded;

        if (isInRange && !isJumping)
            HookPlayer();
    }

    private void HookPlayer()
    {
        isTryingToHit = false;

        PlayerStateMachine player = PlayerStateMachine.instance;
        player.playerLocked.SetLockState(player, PlayerLocked.LockState.Full);

        if (hookSequence.isAlive)
            hookSequence.Stop();

        hookHead.position = player.position;
        float distanceFromBoss = hookHead.localPosition.magnitude;

        Vector3 targetPosition = transform.position;
        if (distanceFromBoss > pullPlayerDistance)
            targetPosition = hookHead.position - hookHead.localPosition.normalized * pullPlayerDistance;

        Sequence.Create()
            .Chain(Tween.Position(hookHead, targetPosition, pullPlayerDuration, Ease.InQuad))
            .Group(Tween.Position(player.transform, targetPosition, pullPlayerDuration, Ease.InQuad))
            .ChainCallback(() => player.playerLocked.UnlockPlayer(player))
            .Chain(Tween.LocalPosition(hookHead, Vector3.zero, 0.1f))
            .ChainCallback(() => DestroyHook());
    }

    private void DestroyHook()
    {
        Destroy(gameObject);
    }
}
