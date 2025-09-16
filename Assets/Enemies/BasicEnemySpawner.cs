using System;
using System.Collections;
using System.Collections.Generic;
using Decor.Door;
using Game_Manager;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    [Serializable]
    public class Wave
    {
        public float timeToSpawn;
        public List<GameObject> enemies;
    }
    
    public class BasicEnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<Wave> waves;
        [SerializeField] private List<DoorController> spawnPositions;

        public static BasicEnemySpawner instance;

        private bool isSpawning;
        
        private void Awake()
        {
            instance = this;
        }

        public void StartSpawning()
        {
            if (isSpawning)
                return;

            isSpawning = true;
            StartCoroutine(SpawnWaves());
        }

        private IEnumerator SpawnWaves()
        {
            foreach (Wave wave in waves)
            {
                yield return new WaitForSeconds(wave.timeToSpawn);

                foreach (GameObject enemy in wave.enemies)
                {
                    SpawnEnemy(enemy);
                }
            }

            yield return new WaitWhile(() => EnemyHolder.instance.isAtLeastAnEnemyAlive);
            GameManager.OnKillLastEnemy.Invoke();
        }

        private void SpawnEnemy(GameObject enemy)
        {
            DoorController door = ChooseRandomDoor();
            
            Instantiate(enemy, door.ComputeSpawnPosition(), Quaternion.identity);
            door.OpenForEnemy();
        }

        private DoorController ChooseRandomDoor()
        {
            int index = Random.Range(0, spawnPositions.Count);
            return spawnPositions[index];
        }
    }
}
