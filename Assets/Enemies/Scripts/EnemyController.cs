using System.Collections.Generic;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using Enemies.Spawner;
using UnityEngine;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    public GameObject deathBehaviourObject;
    public List<EnemyPhase> enemyPhases;
    public SpriteRenderer sprite;
    public SpriteRenderer shadowSprite;
    public Animator animator { get; private set; }
    public Damageable damageable { get; private set; }
    public AfterImage afterImage { get; private set; }


    [HideInInspector] public UnityEvent OnChangeBehaviour = new UnityEvent();

    public List<IEnemyBehaviour> enemyBehaviours;
    public IEnemyBehaviour currentBehaviour { get; private set; }
    public IEnemyBehaviour startingBehaviour { get; private set; }
    public IEnemyBehaviour phaseTransitionBehaviour { get; private set; }

    public int currentPhase { get; private set; } = 0;
    private bool isLastPhase => currentPhase >= enemyPhases.Count - 1;

    private SphereCollider sphereCollider;

    protected virtual void Start()
    {
        animator = sprite.GetComponent<Animator>();
        sphereCollider = GetComponent<SphereCollider>();
        damageable = GetComponent<Damageable>();
        afterImage = GetComponent<AfterImage>();

        damageable.OnTakeDamage.AddListener(() =>
        {
            if (!isLastPhase && currentBehaviour != startingBehaviour && damageable.currentHealth <= enemyPhases[currentPhase + 1].healthThresholdToTriggerTransition)
            {
                Debug.Log("Trigger Next Phase !");
                currentPhase += 1;
                enemyBehaviours = enemyPhases[currentPhase].GetBehaviours();
                CancelCurrentBehaviour();
                startingBehaviour = enemyPhases[currentPhase].GetTransitionBehaviour();
                ChangeBehaviour(startingBehaviour);
            }
        });

        damageable.OnDie.AddListener(() =>
        {
            CancelCurrentBehaviour();
            ChangeBehaviour(deathBehaviourObject.GetComponent<IEnemyBehaviour>());
        });

        enemyBehaviours = enemyPhases[currentPhase].GetBehaviours();

        startingBehaviour = enemyPhases[currentPhase].GetTransitionBehaviour();
        ChangeBehaviour(startingBehaviour);
    }

    private void CancelCurrentBehaviour()
    {
        currentBehaviour.CancelBehaviour(this);
    }

    protected virtual void Update()
    {
        if (currentBehaviour != null)
            currentBehaviour.UpdateBehaviour(this);
    }

    public void ChangeBehaviour(IEnemyBehaviour newBehaviour)
    {
        if (newBehaviour == null || newBehaviour == currentBehaviour)
            return;

        if (currentBehaviour != null)
            currentBehaviour.StopBehaviour(this);
        currentBehaviour = newBehaviour;
        currentBehaviour.SetSubBehaviourState(false);
        currentBehaviour.StartBehaviour(this);

        OnChangeBehaviour?.Invoke();
    }

    public void SelectNewBehaviour(bool isFromTransition = false)
    {
        bool isInTransition = currentBehaviour == startingBehaviour;

        if (isInTransition && !isFromTransition)
            return;

        int count = isInTransition ? enemyBehaviours.Count : enemyBehaviours.Count - 1;
        int randomBehaviourIndex = UnityEngine.Random.Range(0, count);

        if (enemyBehaviours[randomBehaviourIndex] == currentBehaviour)
            randomBehaviourIndex += 1;

        ChangeBehaviour(enemyBehaviours[randomBehaviourIndex]);
    }

    public void DeactivateHitbox()
    {
        EnemyHolder.instance.UnRegisterEnemy(gameObject);
        sphereCollider.enabled = false;
    }

    public void ActivateHitbox()
    {
        EnemyHolder.instance.RegisterEnemy(gameObject);
        sphereCollider.enabled = true;
    }
}
