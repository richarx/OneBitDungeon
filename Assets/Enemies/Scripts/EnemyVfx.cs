using System.Collections.Generic;
using Enemies.Scripts.Behaviours;
using UnityEngine;

namespace Enemies.Scripts
{
    public class EnemyVfx : MonoBehaviour
    {
        [SerializeField] private List<GameObject> bloodPrefabs;
        [SerializeField] private float offset;

        private EnemyStateMachine _enemy;

        private void Start()
        {
            _enemy = GetComponent<EnemyStateMachine>();
            
            _enemy.damageable.OnDie.AddListener(SpawnDeathVfx);
        }

        private void SpawnDeathVfx()
        {
            Vector3 position = transform.position + Vector3.up * offset;
            Quaternion rotation = transform.rotation;

            for (int i = 0; i < 3; i++)
            {
                int index = Random.Range(0, bloodPrefabs.Count);
                Instantiate(bloodPrefabs[index], position + Random.insideUnitSphere, rotation);
            }
        }
    }
}
