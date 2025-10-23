using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Spawner
{
    public class EnemyHolder : MonoBehaviour
    {
        public static EnemyHolder instance;

        private List<GameObject> enemies = new List<GameObject>();
        public List<GameObject> Enemies => enemies;

        public bool isAtLeastAnEnemyAlive => enemies.Count > 0;
        
        private void Awake()
        {
            instance = this;
        }

        public void RegisterEnemy(GameObject enemy)
        {
            if (!enemies.Contains(enemy))
                enemies.Add(enemy);
        }

        public void UnRegisterEnemy(GameObject enemy)
        {
            if (enemies.Contains(enemy))
                enemies.Remove(enemy);
        }
    }
}
