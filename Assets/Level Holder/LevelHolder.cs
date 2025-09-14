using System.Collections;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Level_Holder
{
    public class LevelHolder : MonoBehaviour
    {
        [SerializeField] private Image blackScreen; 
        
        public static UnityEvent OnResetGame = new UnityEvent();
        public static UnityEvent OnRestartGame = new UnityEvent();
        
        private void Start()
        {
            PlayerStateMachine.instance.playerHealth.OnPlayerDie.AddListener(ResetGame);
        }

        private void ResetGame()
        {
            StopAllCoroutines();
            StartCoroutine(ResetGameCoroutine());
        }

        private IEnumerator ResetGameCoroutine()
        {
            yield return new WaitForSeconds(1.0f);
            yield return Tools.Fade(blackScreen, 1.0f, true);
            OnResetGame?.Invoke();
            yield return new WaitForSeconds(0.1f);
            yield return Tools.Fade(blackScreen, 1.0f, false);
            OnRestartGame?.Invoke();
        }
    }
}
