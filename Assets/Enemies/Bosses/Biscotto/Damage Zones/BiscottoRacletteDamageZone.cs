using Player.Scripts;
using PrimeTween;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(DealDamageToPlayer))]
public sealed class BiscottoRacletteDamageZone : MonoBehaviour
{
    private const float ColorTransitionDuration = 0.05f;

    [SerializeField] private Ease spawnEase;
    [SerializeField] private Ease fillEase;
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
    private PlayerStateMachine player;
    private Sequence currentSequence;

    private float radius;
    private float hitStaggerPower;
    private bool isDamageActive;
    private bool hasHitPlayer;

    public bool IsDestroyed { get; private set; }

    public void Setup(float attackRadius, float spawnDuration, float fillDuration, float activeDuration, float staggerPower)
    {
        radius = attackRadius;
        hitStaggerPower = Mathf.Max(0.0f, staggerPower);
        dealDamageToPlayer = GetComponent<DealDamageToPlayer>();
        player = PlayerStateMachine.instance;

        if (player == null)
        {
            Debug.LogError("[BiscottoRacletteDamageZone] Aucun joueur n'est disponible.", this);
            DestroyZone();
            return;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.material = new Material(spriteRenderer.material);

        Material material = spriteRenderer.material;
        int alphaId = Shader.PropertyToID("_alpha");
        int radiusId = Shader.PropertyToID("_Radius");
        int inlineId = Shader.PropertyToID("_InlineThickness");
        int inlineColorId = Shader.PropertyToID("_InlineColor");
        int outlineColorId = Shader.PropertyToID("_OutlineColor");

        material.SetFloat(alphaId, 0.0f);
        material.SetFloat(radiusId, 0.0f);

        float safeSpawnDuration = Mathf.Max(0.0f, spawnDuration);
        float safeFillDuration = Mathf.Max(0.0f, fillDuration);
        float closeDodgeWindowDuration = player.playerData.closeDodgeWindowDuration;
        float dangerFillDuration = withoutCloseWindow
            ? Mathf.Max(0.0f, safeFillDuration - closeDodgeWindowDuration)
            : safeFillDuration;
        float safeActiveDuration = Mathf.Max(ColorTransitionDuration, activeDuration);
        float damageTimestamp = Time.time + safeSpawnDuration + safeFillDuration;

        closeDodgeSession = new CloseDodgeSession(1);
        closeDodgeDetector = new CloseDodgeDetector();
        closeDodgeDetector.Setup(
            damageTimestamp,
            player.playerData.closeDodgeWindowDuration,
            player.playerData.arroganceGainOnCloseDodge,
            this,
            closeDodgeSession);

        currentSequence = Sequence.Create()
            .Chain(Tween.MaterialProperty(material, alphaId, 1.0f, safeSpawnDuration))
            .Group(Tween.MaterialProperty(material, radiusId, radius, safeSpawnDuration, spawnEase));

        currentSequence.Chain(Tween.MaterialProperty(material, inlineId, radius, dangerFillDuration, fillEase));

        if (withoutCloseWindow)
            currentSequence.ChainDelay(closeDodgeWindowDuration);

        currentSequence
            .ChainCallback(ActivateDamage)
            .Chain(Tween.MaterialColor(material, inlineColorId, flashColor, ColorTransitionDuration))
            .Group(Tween.MaterialColor(material, outlineColorId, flashOutlineColor, ColorTransitionDuration))
            .ChainDelay(safeActiveDuration - ColorTransitionDuration)
            .ChainCallback(FinishDamageWindow)
            .Chain(Tween.MaterialColor(material, inlineColorId, filledColor, 0.1f))
            .Group(Tween.MaterialColor(material, outlineColorId, filledOutlineColor, 0.1f))
            .Group(Tween.MaterialProperty(material, alphaId, 0.01f, despawnDuration * 0.9f))
            .Group(Tween.Scale(transform, 0.0f, despawnDuration, Ease.InBack))
            .ChainCallback(DestroyZone);
    }

    public void Cancel()
    {
        closeDodgeDetector?.Cancel();
        closeDodgeSession?.Cancel();

        if (currentSequence.isAlive)
            currentSequence.Stop();

        isDamageActive = false;
        DestroyZone();
    }

    private void Update()
    {
        if (IsDestroyed || player == null)
            return;

        bool isPlayerInside = IsPlayerInside();
        closeDodgeDetector?.Update(isPlayerInside, player.isInArroganceMode);

        if (isDamageActive && !hasHitPlayer && isPlayerInside)
            TryDamagePlayer();
    }

    private void ActivateDamage()
    {
        if (player == null)
            return;

        bool isPlayerInside = IsPlayerInside();
        closeDodgeDetector?.Resolve(isPlayerInside, player.isInArroganceMode);
        isDamageActive = true;

        if (isPlayerInside)
            TryDamagePlayer();

        // Publish the close-dodge result at the damage timestamp, while the
        // player spin is still recent enough for the Arrogant Dodge feedback.
        closeDodgeSession?.CompleteDamageCheck();
    }

    private void TryDamagePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0.0f;

        if (direction.sqrMagnitude <= 0.0001f)
            direction = Vector3.forward;

        hasHitPlayer = dealDamageToPlayer.TryDealDamage(direction, hitStaggerPower);

        if (hasHitPlayer)
            closeDodgeSession?.RegisterHit();
    }

    private void FinishDamageWindow()
    {
        isDamageActive = false;
    }

    private bool IsPlayerInside()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0.0f;
        float damageDistance = radius * transform.localScale.x + player.hitBoxRadius;
        return direction.magnitude <= damageDistance;
    }

    private void DestroyZone()
    {
        if (IsDestroyed)
            return;

        IsDestroyed = true;
        Destroy(gameObject);
    }
}
