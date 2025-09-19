using System;
using System.Collections;
using System.Collections.Generic;
using Decor.Door;
using Game_Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    [Serializable]
    public class Wave
    {
        public float timeToSpawn;
        public bool waitUntilPreviousWaveIsDead;
        public List<GameObject> enemies;
    }
    
    public class BasicEnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<Wave> waves;
        [SerializeField] private bool isDisabled;

        public static BasicEnemySpawner instance;

        public bool IsDisabled => isDisabled;
        
        private bool isSpawning;
        
        private void Awake()
        {
            instance = this;
            
            GameManager.OnResetLevel.AddListener(StopSpawning);
        }

        private void StopSpawning()
        {
            StopAllCoroutines();
            isSpawning = false;
        }

        public void StartSpawning()
        {
            if (isSpawning || isDisabled)
                return;

            isSpawning = true;
            StopAllCoroutines();
            StartCoroutine(SpawnWaves());
        }

        private IEnumerator SpawnWaves()
        {
            foreach (Wave wave in waves)
            {
                if (wave.waitUntilPreviousWaveIsDead)
                    yield return new WaitWhile(() => EnemyHolder.instance.isAtLeastAnEnemyAlive);
                yield return new WaitForSeconds(wave.timeToSpawn);

                foreach (GameObject enemy in wave.enemies)
                {
                    SpawnEnemy(enemy);
                }
            }
            isSpawning = false;

            yield return new WaitWhile(() => EnemyHolder.instance.isAtLeastAnEnemyAlive);
            GameManager.OnKillLastEnemy.Invoke();
        }

        private void SpawnEnemy(GameObject enemy)
        {
            DoorController door = DoorsHolder.instance.GetRandomDoor();
            
            Instantiate(enemy, door.ComputeSpawnPosition(), Quaternion.identity);
            door.OpenForEnemy();
        }
    }
}
