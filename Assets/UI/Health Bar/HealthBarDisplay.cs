using Player.Scripts;
using UnityEngine;

namespace UI.Health_Bar
{
    public class HealthBarDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject healthPointPrefab;
        [SerializeField] private Transform healthPointsHolder;
        [SerializeField] private GameObject pivot;

        private PlayerStateMachine player;
        private float nextUpdateTimestamp = -1.0f;

        private bool isDisplayed = true;
        
        private void Awake()
        {
            PlayerLocked.OnLockPlayer.AddListener(() =>
            {
                if (isDisplayed && PlayerStateMachine.instance.playerLocked.GetLockState == PlayerLocked.LockState.Hidden)
                    HideInstant();
            });
            PlayerLocked.OnUnlockPlayer.AddListener(() =>
            {
                if (!isDisplayed)
                    DisplayHealthBar();
            });
        }

        private void Start()
        {
            player = PlayerStateMachine.instance;
            
            if (isDisplayed)
                SetupHealthBar();
        }

        private void Update()
        {
            if (nextUpdateTimestamp > 0.0f && Time.time <= nextUpdateTimestamp)
                return;
            
            int current = player.playerHealth.CurrentHealth;
            int currentlyDisplayed = healthPointsHolder.childCount;
            
            if (current > currentlyDisplayed)
                SpawnHealthPoint();
            else if (current < currentlyDisplayed)
                RemoveHealthPoints();
        }
        
        private void SetupHealthBar()
        {
            int current = player.playerHealth.CurrentHealth;

            for (int i = 0; i < current; i++)
            {
                Instantiate(healthPointPrefab, Vector3.zero, Quaternion.identity, healthPointsHolder);
            }
        }

        private void SpawnHealthPoint()
        {
            GameObject healthPoint = Instantiate(healthPointPrefab, Vector3.zero, Quaternion.identity, healthPointsHolder);
            if (!player.isLockedAndHidden)
                healthPoint.GetComponent<Animator>().Play("Spawn");

            nextUpdateTimestamp = Time.time + 0.1f;
        }

        private void RemoveHealthPoints()
        {
            GameObject toBeRemoved = healthPointsHolder.GetChild(healthPointsHolder.childCount - 1).gameObject;
            toBeRemoved.GetComponent<Animator>().Play("Remove");
            Destroy(toBeRemoved, 0.3f);

            nextUpdateTimestamp = Time.time + 0.4f;
        }
        
        private void DisplayHealthBar()
        {
            pivot.SetActive(true);
            isDisplayed = true;
        }

        private void HideInstant()
        {
            pivot.SetActive(false);
            isDisplayed = false;
        }
    }
}
