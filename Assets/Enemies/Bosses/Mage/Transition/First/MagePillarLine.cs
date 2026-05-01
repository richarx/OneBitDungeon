using System;
using System.Collections;
using Enemies.Scripts;
using PrimeTween;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class MagePillarLine : MonoBehaviour
{
    [SerializeField] private float targetVerticalOffset;
    [SerializeField] private float targetSmoothTime;

    private LineRenderer lineRenderer;
    private bool isSetup;

    private Transform target;

    private Vector3 startingPosition;
    private Vector3 targetPosition;
    private Vector3 velocity;
    private Vector3 removeVelocity;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;

        GetComponent<Damageable>().OnDie.AddListener(RemoveLine);
    }

    public void SetTarget(Transform newTarget, Vector3 startPos)
    {
        target = newTarget;
        startingPosition = startPos;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { startingPosition, startingPosition });
        isSetup = true;
    }

    private void RemoveLine()
    {
        isSetup = false;
        StartCoroutine(RemoveLineCoroutine());
    }

    private IEnumerator RemoveLineCoroutine()
    {
        float timer = 0.0f;
        while (timer <= 0.3f)
        {
            targetPosition = Vector3.SmoothDamp(targetPosition, target.position + Vector3.up * targetVerticalOffset, ref velocity, targetSmoothTime);
            startingPosition = Vector3.SmoothDamp(startingPosition, targetPosition, ref removeVelocity, targetSmoothTime);
            lineRenderer.SetPositions(new Vector3[] { startingPosition, targetPosition });
            yield return null;
            timer += Time.deltaTime;
        }
        lineRenderer.positionCount = 0;
    }

    private void LateUpdate()
    {
        if (!isSetup)
            return;

        targetPosition = Vector3.SmoothDamp(targetPosition, target.position + Vector3.up * targetVerticalOffset, ref velocity, targetSmoothTime);
        lineRenderer.SetPosition(1, targetPosition);
    }
}
