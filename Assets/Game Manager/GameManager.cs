using System.Collections;
using Decor.Door;
using Enemies;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Decor.Door.DoorController;

namespace Game_Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Image blackScreen;
        
        public static UnityEvent OnKillLastEnemy = new UnityEvent();
        public static UnityEvent OnLockLevel = new UnityEvent();
        public static UnityEvent OnUnlockLevel = new UnityEvent();
        public static UnityEvent OnResetLevel = new UnityEvent();
        public static UnityEvent OnRestartLevel = new UnityEvent();
        
        public static GameManager instance;

        private bool isLevelLocked = false;
        public bool IsLevelLocked => isLevelLocked;
        
        private void Awake()
        {
            instance = this;
            
            OnKillLastEnemy?.AddListener(UnlockLevel);
        }

        private IEnumerator Start()
        {
            PlayerStateMachine.instance.playerHealth.OnPlayerDie.AddListener(RestartLevel);
            
            blackScreen.gameObject.SetActive(true);
            yield return Tools.Fade(blackScreen, 1.0f, false);
            yield return new WaitForSeconds(0.5f);
            LockLevel();
            UnlockPlayer();
            yield return new WaitForSeconds(1.5f);
            BasicEnemySpawner.instance.StartSpawning();
        }

        private static void UnlockPlayer()
        {
            PlayerStateMachine.instance.ChangeBehaviour(PlayerStateMachine.instance.playerIdle);
        }

        private void LockLevel()
        {
            if (isLevelLocked)
                return;
            
            isLevelLocked = true;
            OnLockLevel?.Invoke();
        }

        private void UnlockLevel()
        {
            if (!isLevelLocked)
                return;
            
            isLevelLocked = false;
            OnUnlockLevel?.Invoke();
        }

        private void RestartLevel()
        {
            StopAllCoroutines();
            StartCoroutine(RestartLevelCoroutine());
        }

        private IEnumerator RestartLevelCoroutine()
        {
            yield return new WaitForSeconds(1.0f);
            yield return Tools.Fade(blackScreen, 1.0f, true);
            OnResetLevel?.Invoke();
            yield return new WaitForSeconds(0.1f);
            yield return Tools.Fade(blackScreen, 1.0f, false);
            OnRestartLevel?.Invoke();
            BasicEnemySpawner.instance.StartSpawning();
        }

        public void ChangeScene(string targetSceneName, DoorSide targetDoor)
        {
            StopAllCoroutines();
            StartCoroutine(ChangeSceneCoroutine(targetSceneName, targetDoor));
        }

        private IEnumerator ChangeSceneCoroutine(string targetSceneName, DoorSide targetDoor)
        {
            PlayerStateMachine player = PlayerStateMachine.instance;
            
            player.playerLocked.SetLockState(player);
            yield return Tools.Fade(blackScreen, 0.5f, true);

            AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);

            yield return new WaitUntil(() => operation.isDone);
            isLevelLocked = false;
            yield return null;

            DoorController door = DoorsHolder.instance.GetDoor(targetDoor);
            player.transform.position = door.ComputeSpawnPosition();
            
            yield return Tools.Fade(blackScreen, 0.5f, false);

            yield return new WaitForSeconds(0.5f);

            UnlockPlayer();
            
            if (BasicEnemySpawner.instance != null && !BasicEnemySpawner.instance.IsDisabled)
            {
                LockLevel();
                yield return new WaitForSeconds(1.5f);
                BasicEnemySpawner.instance.StartSpawning();
            }
        }
    }
}
