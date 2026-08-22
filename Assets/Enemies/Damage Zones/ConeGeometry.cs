using UnityEngine;

/// <summary>
/// Geometry helpers for a circular sector in the XZ plane (represented as Vector2).
/// The sector starts at <paramref name="origin"/>, points along <paramref name="forward"/>,
/// and uses an opening angle in degrees.
/// </summary>
public static class ConeGeometry
{
    private const float Epsilon = 0.00001f;

    /// <summary>
    /// Tests a circular hitbox against a closed circular sector exactly. The hitbox is
    /// considered hit when its centre is in the sector, or when it overlaps either
    /// radial edge or the outer arc.
    /// </summary>
    public static bool CircleIntersectsCone(
        Vector2 origin,
        Vector2 forward,
        float radius,
        float openingAngleDegrees,
        Vector2 circleCenter,
        float circleRadius)
    {
        radius = Mathf.Max(0.0f, radius);
        circleRadius = Mathf.Max(0.0f, circleRadius);

        Vector2 normalizedForward = NormalizeForward(forward);
        float halfAngleRadians = Mathf.Clamp(openingAngleDegrees, 0.0f, 360.0f) * 0.5f * Mathf.Deg2Rad;

        if (halfAngleRadians >= Mathf.PI - Epsilon)
            return (circleCenter - origin).sqrMagnitude <= (radius + circleRadius) * (radius + circleRadius);

        if (IsPointInCone(origin, normalizedForward, radius, halfAngleRadians, circleCenter))
            return true;

        Vector2 leftEdge = origin + Rotate(normalizedForward, halfAngleRadians) * radius;
        Vector2 rightEdge = origin + Rotate(normalizedForward, -halfAngleRadians) * radius;

        float minimumDistance = Mathf.Min(
            DistanceToSegment(circleCenter, origin, leftEdge),
            DistanceToSegment(circleCenter, origin, rightEdge));

        Vector2 centreToCircle = circleCenter - origin;
        if (centreToCircle.sqrMagnitude > Epsilon * Epsilon)
        {
            float signedAngle = Vector2.SignedAngle(normalizedForward, centreToCircle);
            if (Mathf.Abs(signedAngle) <= halfAngleRadians * Mathf.Rad2Deg + Epsilon)
                minimumDistance = Mathf.Min(minimumDistance, Mathf.Abs(centreToCircle.magnitude - radius));
        }

        return minimumDistance <= circleRadius + Epsilon;
    }

    /// <summary>
    /// Tests whether a point belongs to a closed circular sector. This method is
    /// public to make gameplay and EditMode tests use the exact same definition.
    /// </summary>
    public static bool PointIsInsideCone(
        Vector2 origin,
        Vector2 forward,
        float radius,
        float openingAngleDegrees,
        Vector2 point)
    {
        float halfAngleRadians = Mathf.Clamp(openingAngleDegrees, 0.0f, 360.0f) * 0.5f * Mathf.Deg2Rad;
        return IsPointInCone(origin, NormalizeForward(forward), Mathf.Max(0.0f, radius), halfAngleRadians, point);
    }

    private static bool IsPointInCone(Vector2 origin, Vector2 normalizedForward, float radius, float halfAngleRadians, Vector2 point)
    {
        Vector2 toPoint = point - origin;
        float distanceSquared = toPoint.sqrMagnitude;

        if (distanceSquared > radius * radius + Epsilon)
            return false;

        if (distanceSquared <= Epsilon * Epsilon || halfAngleRadians >= Mathf.PI - Epsilon)
            return true;

        float cosHalfAngle = Mathf.Cos(halfAngleRadians);
        float directionDot = Vector2.Dot(normalizedForward, toPoint / Mathf.Sqrt(distanceSquared));
        return directionDot >= cosHalfAngle - Epsilon;
    }

    private static Vector2 NormalizeForward(Vector2 forward)
    {
        return forward.sqrMagnitude <= Epsilon * Epsilon ? Vector2.right : forward.normalized;
    }

    private static Vector2 Rotate(Vector2 vector, float angleRadians)
    {
        float sine = Mathf.Sin(angleRadians);
        float cosine = Mathf.Cos(angleRadians);
        return new Vector2(
            vector.x * cosine - vector.y * sine,
            vector.x * sine + vector.y * cosine);
    }

    private static float DistanceToSegment(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd)
    {
        Vector2 segment = segmentEnd - segmentStart;
        float segmentLengthSquared = segment.sqrMagnitude;

        if (segmentLengthSquared <= Epsilon * Epsilon)
            return Vector2.Distance(point, segmentStart);

        float t = Mathf.Clamp01(Vector2.Dot(point - segmentStart, segment) / segmentLengthSquared);
        return Vector2.Distance(point, segmentStart + segment * t);
    }
}
