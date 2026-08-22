using NUnit.Framework;
using UnityEngine;

public class ConeGeometryTests
{
    private static readonly Vector2 Origin = Vector2.zero;
    private static readonly Vector2 Forward = Vector2.right;

    [Test]
    public void CircleIntersectsCone_CentreInsideSector_ReturnsTrue()
    {
        Assert.That(Intersects(new Vector2(3.0f, 0.0f), 0.0f), Is.True);
    }

    [Test]
    public void CircleIntersectsCone_CentreBehindCone_ReturnsFalse()
    {
        Assert.That(Intersects(new Vector2(-1.0f, 0.0f), 0.0f), Is.False);
    }

    [Test]
    public void CircleIntersectsCone_CentreOnAngularEdge_ReturnsTrue()
    {
        Vector2 edge = new Vector2(Mathf.Cos(45.0f * Mathf.Deg2Rad), Mathf.Sin(45.0f * Mathf.Deg2Rad)) * 3.0f;
        Assert.That(Intersects(edge, 0.0f), Is.True);
    }

    [Test]
    public void CircleIntersectsCone_OverlapsOuterArc_ReturnsTrue()
    {
        Assert.That(Intersects(new Vector2(5.08f, 0.0f), 0.1f), Is.True);
        Assert.That(Intersects(new Vector2(5.11f, 0.0f), 0.1f), Is.False);
    }

    [Test]
    public void CircleIntersectsCone_HitboxOverlapsRadialEdge_ReturnsTrueWhenCentreIsOutside()
    {
        Vector2 outsideCentre = new Vector2(Mathf.Cos(50.0f * Mathf.Deg2Rad), Mathf.Sin(50.0f * Mathf.Deg2Rad)) * 3.0f;
        Assert.That(Intersects(outsideCentre, 0.0f), Is.False);
        Assert.That(Intersects(outsideCentre, 0.3f), Is.True);
    }

    [Test]
    public void CircleIntersectsCone_UsesDirectionInAnyRotation()
    {
        Assert.That(ConeGeometry.CircleIntersectsCone(Origin, Vector2.up, 5.0f, 90.0f, new Vector2(0.0f, 3.0f), 0.0f), Is.True);
        Assert.That(ConeGeometry.CircleIntersectsCone(Origin, Vector2.up, 5.0f, 90.0f, new Vector2(3.0f, 0.0f), 0.0f), Is.False);
    }

    [TestCase(30.0f, 15.0f, true)]
    [TestCase(30.0f, 16.0f, false)]
    [TestCase(90.0f, 40.0f, true)]
    [TestCase(180.0f, 90.0f, true)]
    [TestCase(180.0f, 100.0f, false)]
    [TestCase(360.0f, 180.0f, true)]
    public void CircleIntersectsCone_RespectsOpeningAngles(float openingAngle, float targetAngle, bool expected)
    {
        Vector2 target = new Vector2(Mathf.Cos(targetAngle * Mathf.Deg2Rad), Mathf.Sin(targetAngle * Mathf.Deg2Rad)) * 3.0f;
        Assert.That(ConeGeometry.CircleIntersectsCone(Origin, Forward, 5.0f, openingAngle, target, 0.0f), Is.EqualTo(expected));
    }

    [Test]
    public void CircleIntersectsCone_ClampsInvalidInputsAndUsesFallbackDirection()
    {
        Assert.That(ConeGeometry.CircleIntersectsCone(Origin, Vector2.zero, -1.0f, 720.0f, new Vector2(0.1f, 0.0f), 0.0f), Is.False);
        Assert.That(ConeGeometry.CircleIntersectsCone(Origin, Vector2.zero, 1.0f, 90.0f, new Vector2(0.5f, 0.0f), 0.0f), Is.True);
        Assert.That(ConeGeometry.CircleIntersectsCone(Origin, Vector2.zero, 1.0f, 360.0f, new Vector2(-0.5f, 0.0f), -2.0f), Is.True);
    }

    private static bool Intersects(Vector2 circleCentre, float circleRadius)
    {
        return ConeGeometry.CircleIntersectsCone(Origin, Forward, 5.0f, 90.0f, circleCentre, circleRadius);
    }
}
