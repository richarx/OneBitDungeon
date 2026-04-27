using System;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
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

    private Vector3 circlePosition1;
    private Vector3 circlePosition2;
    private Vector3 circlePosition3;

    public bool hasDetonated { get; private set; }
    private Sequence currentSequence;

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

        Sequence.Create()
        .Group(Tween.MaterialProperty(spriteRenderer.material, rasiusId, radius, spawnDuration, spawnEase));
    }

    public void Detonate()
    {
        if (hasDetonated)
            return;

        hasDetonated = true;

        int rasiusId_1 = Shader.PropertyToID($"_Shape1Radius");
        int rasiusId_2 = Shader.PropertyToID($"_Shape2Radius");
        int rasiusId_3 = Shader.PropertyToID($"_Shape3Radius");

        int alphaId = Shader.PropertyToID("_alpha");
        int inlineId = Shader.PropertyToID("_InlineThickness");
        int inlineColorId = Shader.PropertyToID("_InlineColor");
        int outlineColorId = Shader.PropertyToID("_OutlineColor");

        currentSequence = Sequence.Create()
        .Chain(Tween.MaterialProperty(spriteRenderer.material, inlineId, radius, fillDuration, fillEase))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.05f))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, flashColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, flashOutlineColor, 0.05f))
        .ChainCallback(() => CheckForPlayerHit())
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

    private void CheckForPlayerHit()
    {
        Vector3 playerPosition = PlayerStateMachine.instance.position;
        playerPosition.y = 0.0f;
        Vector3 direction1 = playerPosition - circlePosition1;
        Vector3 direction2 = playerPosition - circlePosition2;
        Vector3 direction3 = playerPosition - circlePosition3;
        float damageDistance = (radius * transform.localScale.x) + PlayerStateMachine.instance.hitBoxRadius;

        bool damageApplied = false;

        if (direction1.magnitude <= damageDistance)
            damageApplied |= PlayerStateMachine.instance.playerHealth.TakeDamage(1, direction1.normalized);

        if (direction2.magnitude <= damageDistance)
            damageApplied |= PlayerStateMachine.instance.playerHealth.TakeDamage(1, direction2.normalized);

        if (direction3.magnitude <= damageDistance)
            damageApplied |= PlayerStateMachine.instance.playerHealth.TakeDamage(1, direction3.normalized);

        if (!damageApplied)
            CameraShaker.instance.StartShake();
    }

    public void MoveCircle(int circleIndex, Vector2 position)
    {
        if (!isInit)
            Initialize();

        Vector3 rawPosition = new Vector3(position.x, 0.0f, position.y);
        Vector2 convertedPosition = ComputePosition(position);

        if (circleIndex == 0)
        {
            circlePosition1 = rawPosition;
            spriteRenderer.material.SetVector("_Shape1Position", convertedPosition);
        }
        else if (circleIndex == 1)
        {
            circlePosition2 = rawPosition;
            spriteRenderer.material.SetVector("_Shape2Position", convertedPosition);
        }
        else if (circleIndex == 2)
        {
            circlePosition3 = rawPosition;
            spriteRenderer.material.SetVector("_Shape3Position", convertedPosition);
        }
    }

    private Vector2 ComputePosition(Vector2 position)
    {
        position.x /= transform.localScale.x;
        position.y /= transform.localScale.y;

        return position;
    }

    public void Cancel()
    {
        if (!currentSequence.isAlive)
            return;

        int rasiusId_1 = Shader.PropertyToID($"_Shape1Radius");
        int rasiusId_2 = Shader.PropertyToID($"_Shape2Radius");
        int rasiusId_3 = Shader.PropertyToID($"_Shape3Radius");

        int alphaId = Shader.PropertyToID("_alpha");
        int inlineColorId = Shader.PropertyToID("_InlineColor");
        int outlineColorId = Shader.PropertyToID("_OutlineColor");

        currentSequence.Stop();
        currentSequence = Sequence.Create()
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
}
