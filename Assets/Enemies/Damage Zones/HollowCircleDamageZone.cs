using System.Collections;
using System.Collections.Generic;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class HollowCircleDamageZone : MonoBehaviour
{
    [SerializeField] private float startingRadius;
    [SerializeField] private float maxRadius;
    [SerializeField] private float spawnDuration;
    [SerializeField] private Ease spawnEase;
    [SerializeField] private float despawnDuration;

    [Space]
    [SerializeField] private Color filledColor;
    [SerializeField] private Color filledOutlineColor;

    private Sequence currentSequence;
    private SpriteRenderer spriteRenderer;

    private void Update()
    {
        if (spriteRenderer != null)
            CheckForPlayerHit();
    }

    public void Setup()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.material = new Material(spriteRenderer.material);

        spriteRenderer.material.SetFloat("_alpha", 0.0f);
        spriteRenderer.material.SetFloat("_Shape1Radius", startingRadius);
        spriteRenderer.material.SetFloat("_Shape2Radius", startingRadius - 0.01f);

        int alphaId = Shader.PropertyToID("_alpha");
        int outerRadiusId = Shader.PropertyToID("_Shape1Radius");
        int innerRadiusId = Shader.PropertyToID("_Shape2Radius");
        int inlineColorId = Shader.PropertyToID("_InlineColor");
        int outlineColorId = Shader.PropertyToID("_OutlineColor");

        currentSequence = Sequence.Create()
        .Chain(Tween.MaterialProperty(spriteRenderer.material, alphaId, 1.0f, spawnDuration))
        .Group(Tween.MaterialProperty(spriteRenderer.material, outerRadiusId, maxRadius, spawnDuration, spawnEase))
        .Group(Tween.MaterialProperty(spriteRenderer.material, innerRadiusId, maxRadius - 0.01f, spawnDuration, spawnEase))
        .Group(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.1f, startDelay: spawnDuration - despawnDuration))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.1f, startDelay: spawnDuration - despawnDuration))
        .Group(Tween.MaterialProperty(spriteRenderer.material, alphaId, 0.01f, despawnDuration, startDelay: spawnDuration - despawnDuration))
        .ChainCallback(() => Destroy(gameObject));
    }

    public void Cancel()
    {
        if (!currentSequence.isAlive)
            return;

        currentSequence.Stop();
        Destroy(gameObject);
    }

    private void CheckForPlayerHit()
    {
        float outerRadius = spriteRenderer.material.GetFloat("_Shape1Radius");
        float innerRadius = spriteRenderer.material.GetFloat("_Shape2Radius");

        Vector3 direction = PlayerStateMachine.instance.position - transform.position;
        float maxDamageDistance = (outerRadius * transform.localScale.x) + PlayerStateMachine.instance.hitBoxRadius;
        float minDamageDistance = (innerRadius * transform.localScale.x) - PlayerStateMachine.instance.hitBoxRadius;

        if (direction.magnitude <= maxDamageDistance && direction.magnitude >= minDamageDistance)
            PlayerStateMachine.instance.playerHealth.TakeDamage(1, direction.normalized);
    }
}
