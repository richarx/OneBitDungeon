using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Spawner
{
    public class EnemyHolder : MonoBehaviour
    {
        public static EnemyHolder instance;

        private List<GameObject> enemies = new List<GameObject>();
        public List<GameObject> Enemies => enemies;

        private GameObject mainEnemy;
        public GameObject MainEnemy => mainEnemy;

        public bool isAtLeastAnEnemyAlive => enemies.Count > 0;
        public bool isMainEnemyAlive => mainEnemy != null;

        private void Awake()
        {
            instance = this;
        }

        public void RegisterEnemy(GameObject enemy, bool isMainEnemy)
        {
            if (!enemies.Contains(enemy))
                enemies.Add(enemy);

            if (isMainEnemy)
                mainEnemy = enemy;
        }

        public void UnRegisterEnemy(GameObject enemy)
        {
            if (enemies.Contains(enemy))
                enemies.Remove(enemy);

            if (enemy == mainEnemy)
                mainEnemy = null;
        }
    }
}
