using System.Collections;
using Decor.Door;
using Player.Scripts;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static Decor.Door.DoorController;

namespace Game_Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BlackScreenTransition blackScreenTransition;

        public static UnityEvent OnLockLevel = new UnityEvent();
        public static UnityEvent OnUnlockLevel = new UnityEvent();
        public static UnityEvent OnResetLevel = new UnityEvent();
        public static UnityEvent OnRestartLevel = new UnityEvent();
        public static UnityEvent OnChangeScene = new UnityEvent();
        public static UnityEvent OnPrepareToChangeScene = new UnityEvent();
        public static UnityEvent<DoorSide> OnPrepareToLeaveRoom = new UnityEvent<DoorSide>();

        public static GameManager instance;

        private bool isInMainMenu;

        private string currentRespawnScene;

        private void Awake()
        {
            instance = this;
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
            yield return null;

            PlayerStateMachine player = PlayerStateMachine.instance;
            player.playerSit.SitAfterRespawn(player);
            player.playerSit.Lock();
            player.playerDead.OnPlayerDies.AddListener(RestartLevelOnPlayerDeath);
            SetRespawnPosition();

            blackScreenTransition.DisplayInstant();
            yield return new WaitForSeconds(0.5f);

            yield return new WaitUntil(() => PlayerSpawnPosition.instance != null);
            player.TeleportPlayer(PlayerSpawnPosition.instance.GetPosition);

            yield return new WaitForSeconds(0.5f);

            yield return blackScreenTransition.OpenCircle(player.position, 3.0f);

            player.playerSit.Unlock();

            OnChangeScene?.Invoke();
            OnLockLevel?.Invoke();
        }

        private static void UnlockPlayer()
        {
            PlayerStateMachine.instance.ChangeBehaviour(PlayerStateMachine.instance.playerIdle);
        }

        private void RestartLevelOnPlayerDeath()
        {
            StopAllCoroutines();
            StartCoroutine(RestartLevelOnPlayerDeathCoroutine());
        }

        private IEnumerator RestartLevelOnPlayerDeathCoroutine()
        {
            Time.timeScale = 0.2f;
            yield return new WaitForSecondsRealtime(2.0f);
            Time.timeScale = 0.1f;
            OnPrepareToChangeScene?.Invoke();
            yield return blackScreenTransition.FadeIn(2.5f, false);
            Tween.StopAll();
            Time.timeScale = 1.0f;

            PlayerStateMachine player = PlayerStateMachine.instance;
            player.playerSit.SitAfterRespawn(player);
            player.playerSit.Lock();

            AsyncOperation operation = SceneManager.LoadSceneAsync(currentRespawnScene);

            yield return new WaitUntil(() => operation.isDone);
            yield return new WaitForSeconds(0.1f);

            yield return new WaitUntil(() => PlayerSpawnPosition.instance != null);
            player.TeleportPlayer(PlayerSpawnPosition.instance.GetPosition);

            yield return blackScreenTransition.OpenCircle(player.position, 1.0f);

            player.playerSit.Unlock();

            OnRestartLevel?.Invoke();
            OnChangeScene?.Invoke();
            OnLockLevel?.Invoke();
        }

        public void ChangeSceneFromDoor(string targetSceneName, DoorController triggerDoor)
        {
            StopAllCoroutines();
            StartCoroutine(ChangeSceneFromDoorCoroutine(targetSceneName, triggerDoor));
        }

        private IEnumerator ChangeSceneFromDoorCoroutine(string targetSceneName, DoorController triggerDoor)
        {
            PlayerStateMachine player = PlayerStateMachine.instance;

            player.playerLocked.SetLockState(player);
            OnPrepareToChangeScene?.Invoke();
            OnPrepareToLeaveRoom?.Invoke(triggerDoor.doorDirection);
            yield return blackScreenTransition.FadeIn(0.5f);
            Tween.StopAll();

            AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);

            yield return new WaitUntil(() => operation.isDone);
            yield return null;

            DoorController door = DoorsHolder.instance.GetDoor(triggerDoor);
            Vector3 spawnPosition = door.ComputeSpawnPosition();
            player.rb.position = spawnPosition;

            yield return blackScreenTransition.FadeOut(0.5f);

            UnlockPlayer();

            OnChangeScene?.Invoke();
            OnLockLevel?.Invoke();
        }

        public void SetMenuState(bool state)
        {
            isInMainMenu = state;
        }

        public void SetRespawnPosition()
        {
            currentRespawnScene = SceneManager.GetActiveScene().name;
        }
    }
}
