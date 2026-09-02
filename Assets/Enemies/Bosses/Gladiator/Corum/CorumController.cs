using System;
using System.Collections;
using Enemies.Scripts;
using Enemies.Spawner;
using Player.Scripts;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class CorumController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer shadowSpriteRenderer;

    [Space]
    [SerializeField] private float spawnDuration;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackRange;
    [SerializeField] private float runRange;
    [SerializeField] private float runSpeed;

    [Space]
    [SerializeField] private GameObject rectangleDamageZonePrefab;
    [SerializeField] private float damageSpawnDuration;
    [SerializeField] private float damageFillDuration;
    [SerializeField] private float damageLockBeforeImpact;
    [SerializeField] private float rotationDampening;
    [SerializeField] private float attackAnimationDuration;
    [SerializeField] private float attackDashDistance;
    [SerializeField] private float attackDashDuration;

    private Animator animator;

    private bool isDead;
    private float lastAttackTimestamp;
    private float spawnTimestamp;
    private float currentAimEndTimestamp;
    private Sequence attackSequence;
    private Sequence dashSequence;
    private RectangleDamageZone currentDamageZone;
    private Transform currentDamageZoneRoot;
    private Vector3 rotationDirection;
    private bool hasSpawn => Time.time - spawnTimestamp >= spawnDuration;
    private bool isAttacking => attackSequence.isAlive;
    private const float DamageColorTransitionDuration = 0.05f;


    private void Start()
    {
        animator = spriteRenderer.GetComponent<Animator>();
        GetComponent<Damageable>().OnDie.AddListener(Die);

        lastAttackTimestamp = Time.time;
    }

    private void Update()
    {
        if (isAttacking && currentDamageZoneRoot != null && Time.time <= currentAimEndTimestamp)
        {
            rotationDirection = RotateAttackTowardPlayer();
            return;
        }

        if (!hasSpawn || isDead || isAttacking || PlayerStateMachine.instance == null)
            return;

        float distanceToPlayer = (PlayerStateMachine.instance.position - transform.position).magnitude;

        if (Time.time - lastAttackTimestamp >= attackCooldown && distanceToPlayer <= attackRange && !PlayerStateMachine.instance.playerHealth.IsDead)
        {
            AttackPlayer();
            return;
        }

        if (distanceToPlayer >= runRange)
        {
            MoveTowardPlayer();
            return;
        }

        animator.Play("Idle");
        LookTowardsPlayer();
    }

    private void FixedUpdate()
    {
        if (!hasSpawn || isDead || isAttacking || PlayerStateMachine.instance == null)
            return;
    }

    private Vector3 RotateAttackTowardPlayer()
    {
        LookTowardsPlayer();
        Vector3 position = currentDamageZoneRoot.position;
        Vector3 direction = (PlayerStateMachine.instance.position - position).normalized;

        currentDamageZoneRoot.rotation = Quaternion.Slerp(
            currentDamageZoneRoot.rotation,
            Quaternion.LookRotation(direction.ToVector2().AddAngleToDirection(90.0f).ToVector3()),
            Time.deltaTime / rotationDampening
        );

        return direction;
    }

    private void AttackPlayer()
    {
        attackSequence = Sequence.Create()
            .ChainCallback(() => SpawnRectangleZone())
            .ChainDelay(damageSpawnDuration + damageFillDuration - attackAnimationDuration)
            .ChainCallback(() => animator.Play("Attack"))
            .ChainDelay(attackAnimationDuration)
            .ChainCallback(() => DashAttack())
            .ChainDelay(attackDashDuration)
            .ChainCallback(() => animator.Play("Idle"))
            .ChainDelay(0.5f)
            .ChainCallback(() => lastAttackTimestamp = Time.time);
    }

    private void DashAttack()
    {
        Vector3 targetPosition = transform.position + rotationDirection * attackDashDistance;
        targetPosition = ClampPositionInArena(targetPosition);

        dashSequence = Sequence.Create()
            .Chain(Tween.Position(transform, targetPosition, attackDashDuration, Ease.OutCirc));
    }

    private Vector3 ClampPositionInArena(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, -9.0f, 9.0f);
        position.z = Mathf.Clamp(position.z, -9.0f, 9.0f);

        return position;
    }

    private void SpawnRectangleZone()
    {
        GameObject rectangle = GameObject.Instantiate(rectangleDamageZonePrefab, transform.position, Quaternion.identity);
        currentDamageZoneRoot = rectangle.transform;

        currentDamageZone = rectangle.GetComponentInChildren<RectangleDamageZone>();
        if (currentDamageZone == null)
        {
            Debug.LogError("Corum : The rectangle prefab has no RectangleDamageZone.");
            UnityEngine.Object.Destroy(rectangle);
            return;
        }

        currentAimEndTimestamp = Time.time + Mathf.Max(
            0.0f,
            damageSpawnDuration + damageFillDuration + DamageColorTransitionDuration - damageLockBeforeImpact);

        currentDamageZone.Setup(Vector2.right, damageSpawnDuration, damageFillDuration);
    }

    private void MoveTowardPlayer()
    {
        Vector3 position = transform.position;
        Vector3 directionToPlayer = (PlayerStateMachine.instance.position - position).normalized;
        transform.position += directionToPlayer * runSpeed * Time.deltaTime;

        animator.Play("Run");
        LookTowardsPlayer();
    }

    private void LookTowardsPlayer()
    {
        Vector3 directionToPlayer = (PlayerStateMachine.instance.position - transform.position).normalized;
        spriteRenderer.flipX = directionToPlayer.x < 0.0f;
        shadowSpriteRenderer.flipX = directionToPlayer.x < 0.0f;
    }

    private void Die()
    {
        if (isAttacking)
            attackSequence.Stop();

        if (dashSequence.isAlive)
            dashSequence.Stop();

        if (currentDamageZone != null)
            currentDamageZone.Cancel();

        currentDamageZone = null;
        currentDamageZoneRoot = null;
        attackSequence = default;
        isDead = true;
        animator.Play("Die");
        GetComponent<SphereCollider>().enabled = false;
    }
}
