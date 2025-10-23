using System;
using Enemies.Spawner;
using Player.Scripts;
using Player.Sword_Hitboxes;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;
using static Player.Sword_Hitboxes.WeaponAnimationTriggers;

namespace Enemies.Scripts.Behaviours
{
    public class EnemyStateMachine : MonoBehaviour
    {
        public EnemyData enemyData;
        public SpriteRenderer graphics;
        public GameObject corpsePrefab;
        [SerializeField] private WeaponAnimationTriggers weaponAnimationTriggers;
        [SerializeField] private GameObject damageHitBoxPrefab;
        
        [HideInInspector] public UnityEvent<string, Vector2> OnAttack = new UnityEvent<string, Vector2>();
        [HideInInspector] public UnityEvent OnChangeBehaviour = new UnityEvent();
        
        public IEnemyBehaviour currentBehaviour;
        
        // Behaviour States
        public EnemySpawn enemySpawn = new EnemySpawn();
        public EnemyIdle enemyIdle = new EnemyIdle();
        public EnemyWalk enemyWalk = new EnemyWalk();
        public EnemyStrafe enemyStrafe = new EnemyStrafe();
        public EnemyApproach enemyApproach = new EnemyApproach();
        public EnemyStagger enemyStagger = new EnemyStagger();
        public EnemyDead enemyDead = new EnemyDead();
        public IEnemyBehaviour enemyAttack;

        [HideInInspector] public Rigidbody rb;
        [HideInInspector] public Damageable damageable;
        [HideInInspector] public EnemyAnimation enemyAnimation;
        
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
            enemyAnimation = GetComponent<EnemyAnimation>();
            enemyAttack = GetComponent<IEnemyBehaviour>();

            damageable.OnTakeDamage.AddListener(() => ChangeBehaviour(enemyStagger));
            damageable.OnDie.AddListener(() => ChangeBehaviour(enemyDead));
            
            weaponAnimationTriggers.OnSpawnHitbox.AddListener(SpawnDamageHitbox);
            weaponAnimationTriggers.OnRemoveHitbox.AddListener(RemoveDamageHitbox);
            
            EnemyHolder.instance.RegisterEnemy(gameObject);

            lastLookDirection = Vector2.right;

            currentBehaviour = enemySpawn;
            currentBehaviour.StartBehaviour(this, BehaviourType.Idle);
        }

        private void Update()
        {
            currentBehaviour.UpdateBehaviour(this);
        }

        private void FixedUpdate()
        {
            currentBehaviour.FixedUpdateBehaviour(this);
        }

        public virtual void SelectNextBehaviour()
        {
            if (distanceToPlayer > enemyData.distanceToPlayerWalkThreshold)
                ChangeBehaviour(enemyWalk);
            else 
                ChangeBehaviour(enemyAttack);
        }

        public void ChangeBehaviour(IEnemyBehaviour newBehaviour)
        {
            if (newBehaviour == null || newBehaviour == currentBehaviour)
                return;

            BehaviourType previous = currentBehaviour.GetBehaviourType();
            currentBehaviour.StopBehaviour(this, newBehaviour.GetBehaviourType());
            currentBehaviour = newBehaviour;
            OnChangeBehaviour?.Invoke();
            
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

            SpriteRenderer corpse = Instantiate(corpsePrefab, position, Quaternion.identity).GetComponent<SpriteRenderer>();
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
            ChangeBehaviour(enemyStagger);
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
