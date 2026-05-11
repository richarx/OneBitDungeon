using System;
using Player.Scripts;
using PrimeTween;
using TMPro.EditorUtilities;
using Tools_and_Scripts;
using UnityEngine;

public class CircleDamageZone : MonoBehaviour
{
    [SerializeField] private Ease spawnEase;
    [SerializeField] private Ease fillEase;
    [SerializeField] private float despawnDuration;

    [Space]
    [SerializeField] private Color flashColor;
    [SerializeField] private Color flashOutlineColor;
    [SerializeField] private Color filledColor;
    [SerializeField] private Color filledOutlineColor;

    private Sequence currentSequence;

    private bool isCheckingForDamage;

    private float radius;
    private float spawnDuration;
    private float fillDuration;

    public void Setup(float _radius, float _spawnDuration, float _fillDuration)
    {
        radius = _radius;
        spawnDuration = _spawnDuration;
        fillDuration = _fillDuration;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.material = new Material(spriteRenderer.material);

        spriteRenderer.material.SetFloat("_alpha", 0.0f);
        spriteRenderer.material.SetFloat("_Radius", 0.0f);

        int alphaId = Shader.PropertyToID("_alpha");
        int rasiusId = Shader.PropertyToID("_Radius");
        int inlineId = Shader.PropertyToID("_InlineThickness");
        int inlineColorId = Shader.PropertyToID("_InlineColor");
        int outlineColorId = Shader.PropertyToID("_OutlineColor");

        currentSequence = Sequence.Create()
        .Chain(Tween.MaterialProperty(spriteRenderer.material, alphaId, 1.0f, spawnDuration))
        .Group(Tween.MaterialProperty(spriteRenderer.material, rasiusId, radius, spawnDuration, spawnEase))
        .Chain(Tween.MaterialProperty(spriteRenderer.material, inlineId, radius, fillDuration, fillEase))
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.05f))
        .ChainCallback(() => isCheckingForDamage = true)
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, flashColor, 0.05f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, flashOutlineColor, 0.05f))
        .ChainCallback(() => isCheckingForDamage = false)
        .Chain(Tween.MaterialColor(spriteRenderer.material, inlineColorId, filledColor, 0.1f))
        .Group(Tween.MaterialColor(spriteRenderer.material, outlineColorId, filledOutlineColor, 0.1f))
        .Group(Tween.MaterialProperty(spriteRenderer.material, alphaId, 0.01f, despawnDuration * 0.9f))
        .Group(Tween.Scale(transform, 0.0f, despawnDuration, Ease.InBack))
        .ChainCallback(() => Destroy(gameObject));
    }

    public void Cancel()
    {
        if (!currentSequence.isAlive)
            return;

        currentSequence.Stop();
        Destroy(gameObject);
    }

    private void Update()
    {
        if (isCheckingForDamage)
            CheckForPlayerHit();
    }

    private void CheckForPlayerHit()
    {
        Vector3 direction = PlayerStateMachine.instance.position - transform.position;
        float damageDistance = (radius * transform.localScale.x) + PlayerStateMachine.instance.hitBoxRadius;

        bool damageApplied = false;

        if (direction.magnitude <= damageDistance)
            damageApplied = PlayerStateMachine.instance.playerHealth.TakeDamage(1, direction.normalized);

        if (damageApplied)
            isCheckingForDamage = false;
        else
            CameraShaker.instance.StartShake(1 + radius / 10.0f);
    }
}
