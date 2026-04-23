using PrimeTween;
using UnityEngine;

public class CircleDamageZone : MonoBehaviour
{
    [SerializeField] private float radius;
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

    public void Setup()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.material = new Material(spriteRenderer.material);

        spriteRenderer.material.SetFloat("_alpha", 0.0f);
        spriteRenderer.material.SetFloat("_Radius", 0.0f);

        int alphaId = Shader.PropertyToID("_alpha");
        int rasiusId = Shader.PropertyToID("_Radius");
        int inlineId = Shader.PropertyToID("_InlineThickness");
        int inlineColorId = Shader.PropertyToID("_InlineColor");
        int outlineColorId = Shader.PropertyToID("_OutlineColor");

        Sequence.Create()
        .Chain(Tween.MaterialProperty(spriteRenderer.material, alphaId, 1.0f, spawnDuration))
        .Group(Tween.MaterialProperty(spriteRenderer.material, rasiusId, radius, spawnDuration, spawnEase))
        .Chain(Tween.MaterialProperty(spriteRenderer.material, inlineId, radius, fillDuration, fillEase))
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
