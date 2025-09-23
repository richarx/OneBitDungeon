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
        public static UnityEvent OnChangeScene = new UnityEvent();
        
        public static GameManager instance;

        private bool isLevelLocked = false;
        public bool IsLevelLocked => isLevelLocked;

        private bool isInMainMenu;
        
        private void Awake()
        {
            instance = this;
            
            OnKillLastEnemy?.AddListener(UnlockLevel);
        }

        private IEnumerator Start()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name.Contains("MainMenu") || scene.name.Contains("Intro"))
            {
                Debug.Log("[Game Manager] : excluded scene detected - Waiting For trigger at the end of intro.");
                isInMainMenu = true;
                PlayerStateMachine.instance.playerLocked.SetLockState(PlayerStateMachine.instance, PlayerLocked.LockState.Hidden);
                yield return new WaitWhile(() => isInMainMenu);
                Debug.Log("[Game Manager] : Trigger detected - setting up.");
            }

            PlayerStateMachine player = PlayerStateMachine.instance;
            player.ChangeBehaviour(player.playerSit);
            
            player.playerHealth.OnPlayerDie.AddListener(RestartLevel);
            blackScreen.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            
            if (PlayerSpawnPosition.instance != null)
                player.transform.position = PlayerSpawnPosition.instance.GetPosition;

            yield return new WaitForSeconds(0.5f);
            
            yield return Tools.Fade(blackScreen, 1.0f, false);
            
            player.playerSit.Unlock();
            
            OnChangeScene?.Invoke();
            
            if (BasicEnemySpawner.instance != null && !BasicEnemySpawner.instance.IsDisabled)
            {
                LockLevel();
                yield return new WaitForSeconds(1.5f);
                BasicEnemySpawner.instance.StartSpawning();
            }
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
            Vector3 spawnPosition = door.ComputeSpawnPosition();
            player.rb.position = spawnPosition;
            
            yield return Tools.Fade(blackScreen, 0.5f, false);
            
            UnlockPlayer();
            
            OnChangeScene?.Invoke();
            
            if (BasicEnemySpawner.instance != null && !BasicEnemySpawner.instance.IsDisabled)
            {
                LockLevel();
                yield return new WaitForSeconds(1.5f);
                BasicEnemySpawner.instance.StartSpawning();
            }
        }

        public void SetMenuState(bool state)
        {
            isInMainMenu = state;
        }
    }
}
