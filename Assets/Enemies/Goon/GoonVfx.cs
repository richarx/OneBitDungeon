using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Goon
{
    public class GoonVfx : MonoBehaviour
    {
        [SerializeField] private List<GameObject> bloodPrefabs;
        [SerializeField] private float offset;

        private GoonStateMachine goon;

        private void Start()
        {
            goon = GetComponent<GoonStateMachine>();
            
            goon.damageable.OnDie.AddListener(SpawnDeathVfx);
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
