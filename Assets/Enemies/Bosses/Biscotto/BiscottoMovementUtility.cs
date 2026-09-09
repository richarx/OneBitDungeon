using PrimeTween;
using UnityEngine;

public enum BiscottoSideSelection
{
    Random,
    Clockwise,
    CounterClockwise
}

public static class BiscottoMovementUtility
{
    private const float MinimumRadius = 0.0001f;
    private const float ArenaLimit = 9.0f;

    public static Sequence CreateArcMove(
        EnemyController enemy,
        Vector3 destination,
        Vector3 pivotPosition,
        float duration,
        Ease ease = Ease.InOutSine)
    {
        Transform movingTransform = enemy != null ? enemy.transform : null;
        if (movingTransform == null)
            return Sequence.Create().ChainDelay(duration);

        Vector3 startPosition = movingTransform.position;
        Vector3 segmentStartPosition = startPosition;
        float segmentStartProgress = 0.0f;
        bool wasRooted = false;

        return Sequence.Create()
            .Group(Tween.Custom(0.0f, 1.0f, duration, progress =>
            {
                if (!enemy.CanMove)
                {
                    wasRooted = true;
                    return;
                }

                if (wasRooted)
                {
                    // Resume from the frozen position, rather than applying elapsed tween progress as a teleport.
                    segmentStartPosition = movingTransform.position;
                    segmentStartProgress = progress;
                    wasRooted = false;
                }

                float segmentProgress = Mathf.InverseLerp(segmentStartProgress, 1.0f, progress);
                movingTransform.position = ClampToArena(GetArcPosition(segmentStartPosition, destination, pivotPosition, segmentProgress));
            }, ease));
    }

    public static Vector3 ComputeDestination(
        Transform biscottoTransform,
        Vector3 playerPosition,
        float distance)
    {
        Vector3 directionToPlayer = playerPosition - biscottoTransform.position;
        directionToPlayer.y = 0.0f;

        if (directionToPlayer.sqrMagnitude <= MinimumRadius)
            directionToPlayer = biscottoTransform.forward;

        directionToPlayer.Normalize();

        Vector3 destination = playerPosition + Vector3.forward * distance;
        destination.y = biscottoTransform.position.y;
        return ClampToArena(destination);
    }

    public static float GetSideSign(BiscottoSideSelection sideSelection)
    {
        switch (sideSelection)
        {
            case BiscottoSideSelection.Clockwise:
                return 1.0f;
            case BiscottoSideSelection.CounterClockwise:
                return -1.0f;
            default:
                return Random.value < 0.5f ? -1.0f : 1.0f;
        }
    }

    public static Vector3 GetArcPosition(
        Vector3 startPosition,
        Vector3 destination,
        Vector3 pivotPosition,
        float progress)
    {
        Vector2 startOffset = new Vector2(startPosition.x - pivotPosition.x, startPosition.z - pivotPosition.z);
        Vector2 endOffset = new Vector2(destination.x - pivotPosition.x, destination.z - pivotPosition.z);
        float startRadius = startOffset.magnitude;
        float endRadius = endOffset.magnitude;

        if (startRadius <= MinimumRadius || endRadius <= MinimumRadius)
            return Vector3.Lerp(startPosition, destination, progress);

        float startAngle = Mathf.Atan2(startOffset.y, startOffset.x) * Mathf.Rad2Deg;
        float endAngle = Mathf.Atan2(endOffset.y, endOffset.x) * Mathf.Rad2Deg;
        float angle = startAngle + Mathf.DeltaAngle(startAngle, endAngle) * progress;
        float radius = Mathf.Lerp(startRadius, endRadius, progress);
        float angleInRadians = angle * Mathf.Deg2Rad;

        return new Vector3(
            pivotPosition.x + Mathf.Cos(angleInRadians) * radius,
            Mathf.Lerp(startPosition.y, destination.y, progress),
            pivotPosition.z + Mathf.Sin(angleInRadians) * radius);
    }

    private static Vector3 ClampToArena(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, -ArenaLimit, ArenaLimit);
        position.z = Mathf.Clamp(position.z, -ArenaLimit, ArenaLimit);
        return position;
    }
}
