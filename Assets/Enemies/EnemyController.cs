using System.Collections;
using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using UnityEngine;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    public GameObject startingBehaviour;
    public List<GameObject> behaviours;
    public SpriteRenderer sprite;
    public SpriteRenderer shadowSprite;

    [HideInInspector] public UnityEvent OnChangeBehaviour = new UnityEvent();

    public IEnemyBehaviour currentBehaviour { get; private set; }

    protected virtual void Start()
    {
        ChangeBehaviour(startingBehaviour.GetComponent<IEnemyBehaviour>());
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
}
