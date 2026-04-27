using System;
using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using UnityEngine;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    public GameObject startingBehaviourObject;
    public GameObject phaseTransitionBehaviourObject;
    public int phaseTransitionHealthThreshold;
    public GameObject deathBehaviourObject;
    public List<GameObject> behaviours;
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

    public bool isSecondPhase { get; private set; }

    private SphereCollider sphereCollider;

    protected virtual void Start()
    {
        animator = sprite.GetComponent<Animator>();
        sphereCollider = GetComponent<SphereCollider>();
        damageable = GetComponent<Damageable>();
        afterImage = GetComponent<AfterImage>();

        SetupPhaseTransition();

        damageable.OnDie.AddListener(() =>
        {
            ChangeBehaviour(deathBehaviourObject.GetComponent<IEnemyBehaviour>());
        });

        SetupBehaviours();

        startingBehaviour = startingBehaviourObject.GetComponent<IEnemyBehaviour>();
        ChangeBehaviour(startingBehaviour);
    }

    private void SetupPhaseTransition()
    {
        phaseTransitionBehaviour = phaseTransitionBehaviourObject.GetComponent<IEnemyBehaviour>();
        damageable.OnTakeDamage.AddListener(() =>
        {
            if (!isSecondPhase && damageable.currentHealth <= phaseTransitionHealthThreshold)
            {
                Debug.Log("Trigger second phase !");
                isSecondPhase = true;
                ChangeBehaviour(phaseTransitionBehaviour);
            }
        });
    }

    private void SetupBehaviours()
    {
        enemyBehaviours = new List<IEnemyBehaviour>();

        foreach (GameObject behaviour in behaviours)
        {
            IEnemyBehaviour behaviourPrefab = behaviour.GetComponent<IEnemyBehaviour>();
            behaviourPrefab.SetSubBehaviourState(false);
            enemyBehaviours.Add(behaviourPrefab);
        }
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

    public void SelectNewBehaviour()
    {
        int count = currentBehaviour == startingBehaviour ? enemyBehaviours.Count : enemyBehaviours.Count - 1;
        int randomBehaviourIndex = UnityEngine.Random.Range(0, count);

        if (enemyBehaviours[randomBehaviourIndex] == currentBehaviour)
            randomBehaviourIndex += 1;

        ChangeBehaviour(enemyBehaviours[randomBehaviourIndex]);
    }

    public void DeactivateHitbox()
    {
        sphereCollider.enabled = false;
    }

    public void ActivateHitbox()
    {
        sphereCollider.enabled = true;
    }
}
