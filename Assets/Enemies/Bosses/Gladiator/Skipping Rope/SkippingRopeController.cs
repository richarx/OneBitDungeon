using System;
using PrimeTween;
using UnityEngine;

public class SkippingRopeController : MonoBehaviour
{
    [SerializeField] private Transform hookHead;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private RectangleDamageZone damageZone;

    private bool isSetup;
    private Sequence ropeSequence;

    public void Setup(float flyDistance, float flyDuration)
    {
        isSetup = true;

        float damageZoneDistance = flyDistance + 4.0f;

        ropeSequence = Sequence.Create()
            .Group(Tween.LocalPositionZ(hookHead, 1.0f, flyDistance, flyDuration, Ease.OutQuad))
            .Group(Tween.ScaleX(damageZone.transform, 4.0f, damageZoneDistance, flyDuration, Ease.OutQuad))
            .Group(Tween.LocalPositionX(damageZone.transform, 2.0f, damageZoneDistance / 2.0f, flyDuration, Ease.OutQuad));

        damageZone.SimpleSetup();
    }

    public void Retract(float flyDuration)
    {
        if (ropeSequence.isAlive)
            ropeSequence.Stop();

        ropeSequence = Sequence.Create()
            .Chain(Tween.LocalPositionZ(hookHead, 0.0f, flyDuration, Ease.OutQuad))
            .Group(Tween.ScaleX(damageZone.transform, 0.0f, flyDuration, Ease.OutQuad))
            .Group(Tween.LocalPositionX(damageZone.transform, 0.0f, flyDuration, Ease.OutQuad))
            .ChainCallback(() => DestroyRope());
    }

    private void Update()
    {
        if (isSetup)
            UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { hookHead.localPosition, Vector3.zero });
    }

    private void DestroyRope()
    {
        Destroy(gameObject);
    }
}
