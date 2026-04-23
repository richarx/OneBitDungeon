using PrimeTween;
using UnityEngine;

public class ThreeCirclesDamageZone : MonoBehaviour
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

    private SpriteRenderer spriteRenderer;
    private bool isInit;

    private void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material = new Material(spriteRenderer.material);
        isInit = true;
    }

    public void Setup(int circleIndex)
    {
        if (!isInit)
            Initialize();

        if (circleIndex >= 0 && circleIndex <= 2)
            SetupCircle(circleIndex);
    }

    private void SetupCircle(int circleIndex)
    {
        spriteRenderer.material.SetFloat($"_Shape{circleIndex + 1}Radius", 0.0f);

        int rasiusId = Shader.PropertyToID($"_Shape{circleIndex + 1}Radius");
        int inlineId = Shader.PropertyToID("_InlineThickness");

        Sequence.Create()
        .Group(Tween.MaterialProperty(spriteRenderer.material, rasiusId, radius, spawnDuration, spawnEase));
    }

    public void Detonate()
    {
        int rasiusId_1 = Shader.PropertyToID($"_Shape1Radius");
        int rasiusId_2 = Shader.PropertyToID($"_Shape2Radius");
        int rasiusId_3 = Shader.PropertyToID($"_Shape3Radius");

        int alphaId = Shader.PropertyToID("_alpha");
        int inlineId = Shader.PropertyToID("_InlineThickness");
        int inlineColorId = Shader.PropertyToID("_InlineColor");
        int outlineColorId = Shader.PropertyToID("_OutlineColor");

        Sequence.Create()
        .Chain(Tween.MaterialProperty(spriteRenderer.material, inlineId, radius, fillDuration, fillEase))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.05f))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, flashColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, flashOutlineColor, 0.05f))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.1f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.1f))
        .Group(Tween.MaterialProperty(spriteRenderer.material, rasiusId_1, 0.0f, despawnDuration, Ease.InBack))
        .Group(Tween.MaterialProperty(spriteRenderer.material, rasiusId_2, 0.0f, despawnDuration, Ease.InBack))
        .Group(Tween.MaterialProperty(spriteRenderer.material, rasiusId_3, 0.0f, despawnDuration, Ease.InBack))
        .Group(Tween.MaterialProperty(spriteRenderer.material, alphaId, 0.01f, despawnDuration))
        .ChainCallback(() =>
        {
            Destroy(gameObject);
        });
    }

    public void MoveCircle(int circleIndex, Vector2 position)
    {
        if (!isInit)
            Initialize();

        position = ComputePosition(position);

        if (circleIndex == 0)
            spriteRenderer.material.SetVector("_Shape1Position", position);
        else if (circleIndex == 1)
            spriteRenderer.material.SetVector("_Shape2Position", position);
        else if (circleIndex == 2)
            spriteRenderer.material.SetVector("_Shape3Position", position);
    }

    private Vector2 ComputePosition(Vector2 position)
    {
        position.x /= transform.localScale.x;
        position.y /= transform.localScale.y;

        return position;
    }
}
