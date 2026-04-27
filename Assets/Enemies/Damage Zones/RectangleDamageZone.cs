using System;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class RectangleDamageZone : MonoBehaviour
{
    [SerializeField] private Vector2 size;
    [SerializeField] private float spawnDuration;
    [SerializeField] private Ease spawnEase;
    [SerializeField] private float fillDuration;
    [SerializeField] private Ease fillEase;
    [SerializeField] private float despawnDuration;

    [Space]
    [SerializeField] private Color flashColor;
    [SerializeField] private Color flashOutlineColor;
    [SerializeField] private Color filledColor;
    [SerializeField] private Color filledOutlineColor;

    [Space]
    [SerializeField] private bool alsoDestroyParent;

    private Sequence currentSequence;

    public void Setup(Vector2 moveDirection)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.material = new Material(spriteRenderer.material);

        spriteRenderer.material.SetFloat("_alpha", 0.0f);
        Vector2 startingSize = Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y) ? new Vector2(0.0f, size.y) : new Vector2(size.x, 0.0f);
        spriteRenderer.material.SetVector("_Size", startingSize);

        int alphaId = Shader.PropertyToID("_alpha");
        int sizeId = Shader.PropertyToID("_Size");
        int inlineId = Shader.PropertyToID("_InlineThickness");
        int inlineColorId = Shader.PropertyToID("_InlineColor");
        int outlineColorId = Shader.PropertyToID("_OutlineColor");

        float targetPositionX = transform.localPosition.x + size.x * transform.localScale.x * moveDirection.x;
        float targetPositionZ = transform.localPosition.z + size.y * transform.localScale.y * moveDirection.y;

        currentSequence = Sequence.Create()
        .Chain(Tween.MaterialProperty(spriteRenderer.material, alphaId, 1.0f, spawnDuration))
        .Group(Tween.MaterialProperty(spriteRenderer.material, sizeId, size, spawnDuration, spawnEase))
        .Group(Tween.LocalPositionX(transform, targetPositionX, spawnDuration, spawnEase))
        .Group(Tween.LocalPositionZ(transform, targetPositionZ, spawnDuration, spawnEase))
        .Chain(Tween.MaterialProperty(spriteRenderer.material, inlineId, Mathf.Min(size.x, size.y), fillDuration, fillEase))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.05f))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, flashColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, flashOutlineColor, 0.05f))
        .ChainCallback(() => CheckForPlayerHit())
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.1f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.1f))
        .Group(Tween.MaterialProperty(spriteRenderer.material, alphaId, 0.01f, despawnDuration * 0.9f))
        .Group(Tween.Scale(transform, 0.0f, despawnDuration, Ease.InBack))
        .ChainCallback(() =>
        {
            if (alsoDestroyParent)
                Destroy(transform.parent.gameObject);
            else
                Destroy(gameObject);
        });
    }

    private void CheckForPlayerHit()
    {
        Vector3 position = transform.position;
        float X = size.x * transform.localScale.x + PlayerStateMachine.instance.hitBoxRadius;
        float Y = size.y * transform.localScale.y + PlayerStateMachine.instance.hitBoxRadius;

        Vector2 P = PlayerStateMachine.instance.position.ToVector2();
        Vector2 A = (position - transform.right * X + transform.up * Y).ToVector2();
        Vector2 B = (position + transform.right * X + transform.up * Y).ToVector2();
        Vector2 C = (position + transform.right * X - transform.up * Y).ToVector2();
        Vector2 D = (position - transform.right * X - transform.up * Y).ToVector2();

        bool damageApplied = false;

        if (PointInTriangle(P, A, B, C) || PointInTriangle(P, A, C, D))
            damageApplied = PlayerStateMachine.instance.playerHealth.TakeDamage(1, (P.ToVector3() - position).normalized);

        if (!damageApplied)
            CameraShaker.instance.StartShake();
    }

    public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var s = (p0.x - p2.x) * (p.y - p2.y) - (p0.y - p2.y) * (p.x - p2.x);
        var t = (p1.x - p0.x) * (p.y - p0.y) - (p1.y - p0.y) * (p.x - p0.x);

        if ((s < 0) != (t < 0) && s != 0 && t != 0)
            return false;

        var d = (p2.x - p1.x) * (p.y - p1.y) - (p2.y - p1.y) * (p.x - p1.x);
        return d == 0 || (d < 0) == (s + t <= 0);
    }

    /*
    public void OnDrawGizmos()
    {
        if (PlayerStateMachine.instance == null)
            return;

        Gizmos.color = Color.red;

        Vector3 position = transform.position;

        float X = size.x * transform.localScale.x + PlayerStateMachine.instance.hitBoxRadius;
        float Y = size.y * transform.localScale.y + PlayerStateMachine.instance.hitBoxRadius;

        Vector2 P = PlayerStateMachine.instance.position.ToVector2();
        Vector3 A = position - transform.right * X + transform.up * Y;
        Vector3 B = position + transform.right * X + transform.up * Y;
        Vector3 C = position + transform.right * X - transform.up * Y;
        Vector3 D = position - transform.right * X - transform.up * Y;

        Gizmos.DrawSphere(A, 0.1f);
        Gizmos.DrawSphere(B, 0.1f);
        Gizmos.DrawSphere(C, 0.1f);
        Gizmos.DrawSphere(D, 0.1f);
        Gizmos.DrawSphere(P.ToVector3(), 0.25f);
    }
    */

    public void Cancel()
    {
        if (!currentSequence.isAlive)
            return;

        currentSequence.Stop();

        if (alsoDestroyParent)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
}
