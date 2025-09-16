using System;
using Player.Scripts;
using Player.Sword_Hitboxes;
using Tools_and_Scripts;
using UnityEngine;
using static Player.Sword_Hitboxes.WeaponAnimationTriggers;

namespace Enemies.Goon
{
    public class GoonStateMachine : MonoBehaviour
    {
        public GoonData goonData;
        public SpriteRenderer graphics;
        public GameObject goonCorpsePrefab;
        [SerializeField] private WeaponAnimationTriggers weaponAnimationTriggers;
        [SerializeField] private GameObject damageHitBoxPrefab;
        
        public IGoonBehaviour currentBehaviour;
        
        // Behaviour States
        public GoonSpawn goonSpawn = new GoonSpawn();
        public GoonIdle goonIdle = new GoonIdle();
        public GoonWalk goonWalk = new GoonWalk();
        public GoonStrafe goonStrafe = new GoonStrafe();
        public GoonApproach goonApproach = new GoonApproach();
        public GoonSwordAttack goonSwordAttack = new GoonSwordAttack();
        public GoonDash goonDash = new GoonDash();
        public GoonStagger goonStagger = new GoonStagger();
        public GoonDead goonDead = new GoonDead();

        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public Damageable damageable;
        
        private GameObject currentHitbox;
        
        [HideInInspector] public Vector3 moveVelocity;
        private Vector2 lastLookDirection = Vector2.right;
        public Vector2 LastLookDirection => lastLookDirection;
        
        public Vector3 position => transform.position;
        public float distanceToPlayer => (PlayerStateMachine.instance.position - position).magnitude;
        public Vector3 directionToPlayer => (PlayerStateMachine.instance.position - position).normalized;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            damageable = GetComponent<Damageable>();

            damageable.OnTakeDamage.AddListener(() => ChangeBehaviour(goonStagger));
            damageable.OnDie.AddListener(() => ChangeBehaviour(goonDead));
            
            weaponAnimationTriggers.OnSpawnHitbox.AddListener(SpawnDamageHitbox);
            weaponAnimationTriggers.OnRemoveHitbox.AddListener(RemoveDamageHitbox);
            
            EnemyHolder.instance.RegisterEnemy(gameObject);

            lastLookDirection = Vector2.right;

            currentBehaviour = goonSpawn;
            currentBehaviour.StartBehaviour(this, BehaviourType.Idle);
        }

        private void Update()
        {
            currentBehaviour.UpdateBehaviour(this);
            KeepOnGround();
        }
        
        private void KeepOnGround()
        {
            Vector3 currentPosition = transform.position;
            currentPosition.y = 0.0f;
            transform.position = currentPosition;
        }
        
        private void FixedUpdate()
        {
            currentBehaviour.FixedUpdateBehaviour(this);
        }

        public void SelectNextBehaviour()
        {
            if (distanceToPlayer > goonData.distanceToPlayerWalkThreshold)
                ChangeBehaviour(goonWalk);
            else
            {
                if (!damageable.IsFullLife && goonDash.CanDash(this) && Tools.RandomBool())
                    ChangeBehaviour(goonDash);
                else if (goonSwordAttack.CanAttack())
                    ChangeBehaviour(goonApproach);
                else
                    ChangeBehaviour(goonStrafe);
            }
        }

        public void ChangeBehaviour(IGoonBehaviour newBehaviour)
        {
            if (newBehaviour == null || newBehaviour == currentBehaviour)
                return;

            BehaviourType previous = currentBehaviour.GetBehaviourType();
            currentBehaviour.StopBehaviour(this, newBehaviour.GetBehaviourType());
            currentBehaviour = newBehaviour;
            
            currentBehaviour.StartBehaviour(this, previous);
        }
        
        public void ApplyMovement()
        {
            moveVelocity.y = 0.0f;
            rb.velocity = moveVelocity;
        }

        public void SpawnCorpse()
        {
            Destroy(gameObject);

            SpriteRenderer corpse = Instantiate(goonCorpsePrefab, position, Quaternion.identity).GetComponent<SpriteRenderer>();
            corpse.sprite = graphics.sprite;
            corpse.color = graphics.color;
        }

        public void ComputeLastLookDirection()
        {
            lastLookDirection = directionToPlayer.ToVector2().normalized;
        }
        
        public void SetLastLookDirection(Vector2 direction)
        {
            lastLookDirection = direction.normalized;
        }
        
        private void SpawnDamageHitbox(AttackDirection direction)
        {
            if (currentHitbox != null)
                RemoveDamageHitbox();

            currentHitbox = Instantiate(damageHitBoxPrefab, position, Quaternion.identity, transform);
            currentHitbox.transform.RotateAround(position, Vector3.up, ComputeHitboxDirection(direction));
            currentHitbox.transform.GetChild(0).GetComponent<EnemyDealDamage>().OnHitParry.AddListener(GetParried);
        }

        private void GetParried()
        {
            ChangeBehaviour(goonStagger);
        }

        private float ComputeHitboxDirection(AttackDirection direction)
        {
            switch (direction)
            {
                case AttackDirection.L:
                    return 180.0f;
                case AttackDirection.F:
                    return 90.0f;
                case AttackDirection.R:
                    return 0.0f;
                case AttackDirection.BR:
                    return -45.0f;
                case AttackDirection.B:
                    return -100.0f;
                case AttackDirection.BL:
                    return -145.0f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public void RemoveDamageHitbox()
        {
            if (currentHitbox != null)
            {
                Destroy(currentHitbox);
                currentHitbox = null;
            }
        }

        private void OnDestroy()
        {
            if (EnemyHolder.instance != null)
                EnemyHolder.instance.UnRegisterEnemy(gameObject);
        }
    }
}
