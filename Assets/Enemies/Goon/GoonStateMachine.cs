using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;

namespace Enemies.Goon
{
    public class GoonStateMachine : MonoBehaviour
    {
        public GoonData goonData;
        public SpriteRenderer graphics;
        public GameObject goonCorpsePrefab;
        
        public IGoonBehaviour currentBehaviour;
        
        // Behaviour States
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
        
        [HideInInspector] public Vector3 moveVelocity;
        private Vector2 lastLookDirection = Vector2.right;
        public Vector2 LastLookDirection => lastLookDirection;
        
        public Vector3 position => transform.position;
        public float distanceToPlayer => (PlayerStateMachine.instance.position - position).magnitude;
        public Vector3 directionToPlayer => (PlayerStateMachine.instance.position - position).normalized;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            damageable = GetComponent<Damageable>();

            damageable.OnTakeDamage.AddListener(() => ChangeBehaviour(goonStagger));
            damageable.OnDie.AddListener(() => ChangeBehaviour(goonDead));
            
            EnemyHolder.instance.RegisterEnemy(gameObject);
            damageable.OnDie.AddListener(() => EnemyHolder.instance.UnRegisterEnemy(gameObject));
            
            lastLookDirection = Vector2.right;

            currentBehaviour = goonWalk;
            currentBehaviour.StartBehaviour(this, BehaviourType.Idle);
        }

        private void Update()
        {
            currentBehaviour.UpdateBehaviour(this);
            
            transform.position += Vector3.down * transform.position.y;
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
                if (!damageable.IsFullLife && goonDash.CanDash(this))
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
    }
}
