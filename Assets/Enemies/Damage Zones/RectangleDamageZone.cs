using PrimeTween;
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

        float targetPositionX = transform.position.x + size.x * transform.localScale.x * moveDirection.x;
        float targetPositionZ = transform.position.z + size.y * transform.localScale.y * moveDirection.y;

        Sequence.Create()
        .Chain(Tween.MaterialProperty(spriteRenderer.material, alphaId, 1.0f, spawnDuration))
        .Group(Tween.MaterialProperty(spriteRenderer.material, sizeId, size, spawnDuration, spawnEase))
        .Group(Tween.PositionX(transform, targetPositionX, spawnDuration, spawnEase))
        .Group(Tween.PositionZ(transform, targetPositionZ, spawnDuration, spawnEase))
        .Chain(Tween.MaterialProperty(spriteRenderer.material, inlineId, Mathf.Min(size.x, size.y), fillDuration, fillEase))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.05f))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, flashColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, flashOutlineColor, 0.05f))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.1f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.1f))
        .Group(Tween.MaterialProperty(spriteRenderer.material, alphaId, 0.01f, despawnDuration * 0.9f))
        .Group(Tween.Scale(transform, 0.0f, despawnDuration, Ease.InBack))
        .ChainCallback(() => Destroy(gameObject));
    }
}
