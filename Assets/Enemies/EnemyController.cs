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
    public List<GameObject> behaviours;
    public SpriteRenderer sprite;
    public SpriteRenderer shadowSprite;

    [HideInInspector] public UnityEvent OnChangeBehaviour = new UnityEvent();

    public List<IEnemyBehaviour> enemyBehaviours;
    public IEnemyBehaviour currentBehaviour { get; private set; }
    public IEnemyBehaviour startingBehaviour { get; private set; }
    public IEnemyBehaviour phaseTransitionBehaviour { get; private set; }

    public bool isSecondPhase { get; private set; }

    protected virtual void Start()
    {
        /*
        phaseTransitionBehaviour = phaseTransitionBehaviourObject.GetComponent<IEnemyBehaviour>();
        Damageable damageable = GetComponent<Damageable>();
        damageable.OnTakeDamage.AddListener(() =>
        {
            if (!isSecondPhase && damageable.currentHealth <= phaseTransitionHealthThreshold)
            {
                isSecondPhase = true;
                ChangeBehaviour(phaseTransitionBehaviour);
            }
        });
        */

        enemyBehaviours = new List<IEnemyBehaviour>();

        foreach (GameObject behaviour in behaviours)
        {
            enemyBehaviours.Add(behaviour.GetComponent<IEnemyBehaviour>());
        }

        startingBehaviour = startingBehaviourObject.GetComponent<IEnemyBehaviour>();
        ChangeBehaviour(startingBehaviour);


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
}
