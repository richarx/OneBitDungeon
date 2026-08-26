using Player.Scripts;
using PrimeTween;
using UnityEngine;

/// <summary>
/// A telegraphed circular sector damage zone. The local sprite +Y axis is the
/// cone forward axis; Setup rotates that axis into the requested XZ direction.
/// Radius is expressed in world units.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(DealDamageToPlayer))]
public sealed class ConeDamageZone : MonoBehaviour
{
    private const float ColorTransitionDuration = 0.05f;
    private const float MinimumScale = 0.0001f;

    [SerializeField] private Ease spawnEase = Ease.OutQuad;
    [SerializeField] private Ease fillEase = Ease.Linear;
    [SerializeField] private float despawnDuration = 0.3f;
    [SerializeField] private bool withoutCloseWindow;

    [Space]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private Color flashOutlineColor = Color.white;
    [SerializeField] private Color filledColor = Color.black;
    [SerializeField] private Color filledOutlineColor = Color.red;

    private DealDamageToPlayer dealDamageToPlayer;
    private CloseDodgeDetector closeDodgeDetector;
    private CloseDodgeSession closeDodgeSession;
    private PlayerStateMachine playerInstance;
    private Sequence currentSequence;

    private bool isSetup;
    private bool isCheckingForDamage;
    private Vector2 direction;
    private float radius;
    private float openingAngleDegrees;

    public bool IsDestroyed { get; private set; }

    /// <summary>
    /// Configures the zone. Direction is in the XZ plane (x, z), radius is in
    /// world units and openingAngleDegrees is clamped to [0, 360].
    /// </summary>
    public void Setup(
        Vector2 coneDirection,
        float coneRadius,
        float openingAngle,
        float spawnDuration,
        float fillDuration,
        CloseDodgeSession session = null)
    {
        direction = coneDirection.sqrMagnitude <= Mathf.Epsilon ? Vector2.right : coneDirection.normalized;
        radius = Mathf.Max(0.0f, coneRadius);
        openingAngleDegrees = Mathf.Clamp(openingAngle, 0.0f, 360.0f);
        closeDodgeSession = session;

        AlignLocalForwardWithDirection();

        dealDamageToPlayer = GetComponent<DealDamageToPlayer>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material = new Material(spriteRenderer.material);

        ValidateUniformPlaneScale();
        float localRadius = radius / GetHorizontalWorldScale();
        float halfAngleRadians = openingAngleDegrees * 0.5f * Mathf.Deg2Rad;
        Material material = spriteRenderer.material;
        material.SetFloat("_alpha", 0.0f);
        material.SetFloat("_Radius", 0.0f);
        material.SetFloat("_HalfAngle", halfAngleRadians);
        material.SetFloat("_Filling", 0.0f);

        int alphaId = Shader.PropertyToID("_alpha");
        int radiusId = Shader.PropertyToID("_Radius");
        int inlineId = Shader.PropertyToID("_Filling");
        int inlineColorId = Shader.PropertyToID("_InlineColor");
        int outlineColorId = Shader.PropertyToID("_OutlineColor");

        playerInstance = PlayerStateMachine.instance;
        if (playerInstance == null)
        {
            Debug.LogError($"[{nameof(ConeDamageZone)}] A PlayerStateMachine instance is required.", this);
            DestroyZone();
            return;
        }

        float safeSpawnDuration = Mathf.Max(0.0f, spawnDuration);
        float safeFillDuration = Mathf.Max(0.0f, fillDuration);
        float closeDodgeWindowDuration = playerInstance.playerData.closeDodgeWindowDuration;
        float dangerFillDuration = withoutCloseWindow
            ? Mathf.Max(0.0f, safeFillDuration - closeDodgeWindowDuration)
            : safeFillDuration;
        float damageTimestamp = Time.time + safeSpawnDuration + safeFillDuration;
        closeDodgeDetector = new CloseDodgeDetector();
        closeDodgeDetector.Setup(
            damageTimestamp,
            playerInstance.playerData.closeDodgeWindowDuration,
            playerInstance.playerData.arroganceGainOnCloseDodge,
            this,
            closeDodgeSession);

        isSetup = true;
        currentSequence = Sequence.Create()
            .Chain(Tween.MaterialProperty(material, alphaId, 1.0f, safeSpawnDuration))
            .Group(Tween.MaterialProperty(material, radiusId, localRadius, safeSpawnDuration, spawnEase));

        currentSequence.Chain(Tween.MaterialProperty(material, inlineId, 1.0f, dangerFillDuration, fillEase));

        if (withoutCloseWindow)
            currentSequence.ChainDelay(closeDodgeWindowDuration);

        currentSequence
            .Chain(Tween.MaterialColor(material, inlineColorId, filledColor, ColorTransitionDuration))
            .Group(Tween.MaterialColor(material, outlineColorId, filledOutlineColor, ColorTransitionDuration))
            .ChainCallback(() =>
            {
                closeDodgeDetector.Resolve(IsPlayerInside(), playerInstance.isInArroganceMode);
                isCheckingForDamage = true;
                CheckForPlayerHit();
            })
            .Chain(Tween.MaterialColor(material, inlineColorId, flashColor, ColorTransitionDuration))
            .Group(Tween.MaterialColor(material, outlineColorId, flashOutlineColor, ColorTransitionDuration))
            .ChainCallback(() =>
            {
                isCheckingForDamage = false;
                closeDodgeSession?.CompleteDamageCheck();
            })
            .Chain(Tween.MaterialColor(material, inlineColorId, filledColor, 0.1f))
            .Group(Tween.MaterialColor(material, outlineColorId, filledOutlineColor, 0.1f))
            .Group(Tween.MaterialProperty(material, alphaId, 0.01f, Mathf.Max(0.0f, despawnDuration) * 0.9f))
            .Group(Tween.Scale(transform, 0.0f, Mathf.Max(0.0f, despawnDuration), Ease.InBack))
            .ChainCallback(DestroyZone);
    }

    public void Cancel()
    {
        closeDodgeDetector?.Cancel();
        closeDodgeSession?.Cancel();

        if (currentSequence.isAlive)
            currentSequence.Stop();

        isCheckingForDamage = false;
        DestroyZone();
    }

    /// <summary>
    /// Updates the cone facing direction while it is active. This is useful for
    /// telegraphs that must keep tracking the player before they strike.
    /// </summary>
    public void SetDirection(Vector2 coneDirection)
    {
        if (IsDestroyed)
            return;

        direction = coneDirection.sqrMagnitude <= Mathf.Epsilon
            ? direction
            : coneDirection.normalized;

        AlignLocalForwardWithDirection();
    }

    /// <summary>Returns whether the player's circular hitbox overlaps the sector.</summary>
    public bool IsPlayerInside()
    {
        if (playerInstance == null)
            return false;

        Vector3 playerPosition = playerInstance.position;
        return ConeGeometry.CircleIntersectsCone(
            new Vector2(transform.position.x, transform.position.z),
            direction,
            radius,
            openingAngleDegrees,
            new Vector2(playerPosition.x, playerPosition.z),
            playerInstance.hitBoxRadius);
    }

    private void Update()
    {
        if (!isSetup || playerInstance == null || IsDestroyed)
            return;

        closeDodgeDetector?.Update(IsPlayerInside(), playerInstance.isInArroganceMode);

        if (isCheckingForDamage)
            CheckForPlayerHit();
    }

    private void CheckForPlayerHit()
    {
        if (!IsPlayerInside())
            return;

        Vector3 playerPosition = playerInstance.position;
        Vector3 hitDirection = playerPosition - transform.position;
        hitDirection.y = 0.0f;

        if (dealDamageToPlayer.TryDealDamage(hitDirection))
        {
            closeDodgeSession?.RegisterHit();
            isCheckingForDamage = false;
        }
    }

    private void AlignLocalForwardWithDirection()
    {
        // The sprite lies in local XY. X is world-right and local +Y becomes
        // world-forward after the X=90 degree plane rotation.
        transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0.0f, direction.y), Vector3.up)
            * Quaternion.Euler(90.0f, 0.0f, 0.0f);
    }

    private float GetHorizontalWorldScale()
    {
        return Mathf.Max(MinimumScale, Mathf.Abs(transform.lossyScale.x));
    }

    private void ValidateUniformPlaneScale()
    {
        float xScale = Mathf.Abs(transform.lossyScale.x);
        float yScale = Mathf.Abs(transform.lossyScale.y);
        if (!Mathf.Approximately(xScale, yScale))
            Debug.LogWarning($"[{nameof(ConeDamageZone)}] The SpriteRenderer plane must use a uniform XY scale. " +
                             "The prefab uses 20 x 20; a non-uniform override distorts the visual cone.", this);
    }

    private void DestroyZone()
    {
        if (IsDestroyed)
            return;

        IsDestroyed = true;
        Destroy(gameObject);
    }
}
