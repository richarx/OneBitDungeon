using System.Collections.Generic;
using Level_Holder;
using UnityEngine;

namespace Enemies
{
    public class SpawnCorpseOnDeath : MonoBehaviour
    {
        [SerializeField] private List<GameObject> corpsePrefabs;
        [SerializeField] private List<GameObject> vfxPrefabs;

        private void Start()
        {
            Damageable damageable = GetComponent<Damageable>();
            
            if (damageable != null)
                damageable.OnDie.AddListener(DestroyAndSpawnCorpses);
        }

        private void DestroyAndSpawnCorpses()
        {
            Destroy(gameObject);

            Vector3 position = transform.position;

            if (corpsePrefabs.Count > 0)
            {
                int randomCorpse = Random.Range(0, corpsePrefabs.Count);
                Instantiate(corpsePrefabs[randomCorpse], position, Quaternion.identity);
            }

            if (vfxPrefabs.Count > 0)
            {
                int randomVfx = Random.Range(0, vfxPrefabs.Count);
                Instantiate(vfxPrefabs[randomVfx], position, Quaternion.identity);
            }
        }
    }
}
