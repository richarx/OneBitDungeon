using System.Collections.Generic;
using Level_Holder;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class BasicEnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private List<Transform> spawnPositions;
        
        private void Start()
        {
            SpawnEnemy();
            LevelHolder.OnRestartGame.AddListener(SpawnEnemy);
        }

        private void SpawnEnemy()
        {
            Damageable enemy = Instantiate(enemyPrefab, ComputeSpawnPosition(), Quaternion.identity).GetComponent<Damageable>();
            enemy.OnDie.AddListener(SpawnEnemy);
        }

        private Vector3 ComputeSpawnPosition()
        {
            int index = Random.Range(0, spawnPositions.Count);
            return spawnPositions[index].position;
        }
    }
}
