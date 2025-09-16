using System.Collections;
using Enemies;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game_Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Image blackScreen;
        
        public static UnityEvent OnKillLastEnemy = new UnityEvent();
        public static UnityEvent OnLockLevel = new UnityEvent();
        public static UnityEvent OnUnlockLevel = new UnityEvent();

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
            blackScreen.gameObject.SetActive(true);
            yield return Tools.Fade(blackScreen, 1.0f, false);
            yield return new WaitForSeconds(0.5f);
            LockLevel();
            yield return new WaitForSeconds(1.5f);
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
    }
}
