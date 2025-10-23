using Enemies.Scripts.Behaviours;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Enemies.BehaviourDisplayer
{
    public class DisplayCurrentBehaviour : MonoBehaviour
    {
        private TextMeshPro textMeshPro;
        
        private void Start()
        {
            textMeshPro = GetComponent<TextMeshPro>();

            Assert.IsNotNull(transform.parent, $"[{nameof(DisplayCurrentBehaviour)}] : error : transform has no parent.");
            EnemyStateMachine enemy = transform.parent.GetComponent<EnemyStateMachine>();
            Assert.IsNotNull(enemy, $"[{nameof(DisplayCurrentBehaviour)}] : error : should be instantiated as children of an EnemyStateMachine.");
            
            enemy.OnChangeBehaviour.AddListener(() => DisplayBehaviour(enemy.currentBehaviour.GetType().Name));
        }

        private void DisplayBehaviour(string behaviourType)
        {
            textMeshPro.text = $"{behaviourType}";
        }
    }
}
