using PrimeTween;
using UnityEngine;

public static class EnemyMovementUtility
{
    public static Sequence CreateMoveToPosition(
        EnemyController enemy,
        Vector3 destination,
        float duration,
        Ease ease = Ease.InOutSine)
    {
        Transform movingTransform = enemy != null ? enemy.transform : null;
        if (movingTransform == null)
            return Sequence.Create().ChainDelay(duration);

        Vector3 segmentStartPosition = movingTransform.position;
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
                    segmentStartPosition = movingTransform.position;
                    segmentStartProgress = progress;
                    wasRooted = false;
                }

                float segmentProgress = Mathf.InverseLerp(segmentStartProgress, 1.0f, progress);
                movingTransform.position = Vector3.Lerp(segmentStartPosition, destination, segmentProgress);
            }, ease));
    }

    public static Sequence CreateRigidbodyMove(
        EnemyController enemy,
        Rigidbody rigidbody,
        Vector3 destination,
        float duration,
        Ease ease = Ease.InOutSine)
    {
        if (enemy == null || rigidbody == null)
            return Sequence.Create().ChainDelay(duration);

        Vector3 segmentStartPosition = rigidbody.position;
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
                    segmentStartPosition = rigidbody.position;
                    segmentStartProgress = progress;
                    wasRooted = false;
                }

                float segmentProgress = Mathf.InverseLerp(segmentStartProgress, 1.0f, progress);
                rigidbody.MovePosition(Vector3.Lerp(segmentStartPosition, destination, segmentProgress));
            }, ease));
    }
}
