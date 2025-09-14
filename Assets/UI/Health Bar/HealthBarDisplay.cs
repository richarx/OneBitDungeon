using System.Collections;
using Player.Scripts;
using UnityEngine;

namespace UI.Health_Bar
{
    public class HealthBarDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject healthPointPrefab;
        [SerializeField] private Transform healthPointsHolder;
        
        private PlayerHealth playerHealth;

        private float nextUpdateTimestamp = -1.0f;

        private void Start()
        {
            playerHealth = PlayerStateMachine.instance.playerHealth;
            
            SetupHealthBar();
        }

        private void Update()
        {
            if (nextUpdateTimestamp > 0.0f && Time.time <= nextUpdateTimestamp)
                return;
            
            int current = playerHealth.CurrentHealth;
            int currentlyDisplayed = healthPointsHolder.childCount;
            
            if (current > currentlyDisplayed)
                SpawnHealthPoint();
            else if (current < currentlyDisplayed)
                RemoveHealthPoints(currentlyDisplayed - current);
        }
        
        private void SetupHealthBar()
        {
            int current = playerHealth.CurrentHealth;

            for (int i = 0; i < current; i++)
            {
                Instantiate(healthPointPrefab, Vector3.zero, Quaternion.identity, healthPointsHolder);
            }
        }

        private void SpawnHealthPoint()
        {
            GameObject healthPoint = Instantiate(healthPointPrefab, Vector3.zero, Quaternion.identity, healthPointsHolder);
            healthPoint.GetComponent<Animator>().Play("Spawn");

            nextUpdateTimestamp = Time.time + 0.1f;
        }

        private void RemoveHealthPoints(int toRemoveCount)
        {
            for (int i = 0; i < toRemoveCount; i++)
            {
                GameObject toBeRemoved = healthPointsHolder.GetChild(healthPointsHolder.childCount - (1 + i)).gameObject;
                toBeRemoved.GetComponent<Animator>().Play("Remove");
                Destroy(toBeRemoved, 0.3f);
            }

            nextUpdateTimestamp = Time.time + 0.4f;
        }
    }
}
