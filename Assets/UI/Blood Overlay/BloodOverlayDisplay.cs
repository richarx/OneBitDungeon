using System.Collections;
using System.Collections.Generic;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Blood_Overlay
{
    public class BloodOverlayDisplay : MonoBehaviour
    {
        [SerializeField] private Image bloodOverlay;
        [SerializeField] private List<Sprite> sprites;
        [SerializeField] private float maxFade;
        [SerializeField] private float duration;
        
        private void Start()
        {
            PlayerStateMachine.instance.playerHealth.OnPlayerTakeDamage.AddListener((_) => DisplayBloodOverlay());        
        }

        private void DisplayBloodOverlay()
        {
            StopAllCoroutines();
            StartCoroutine(DisplayCoroutine());
        }

        private IEnumerator DisplayCoroutine()
        {
            int index = Random.Range(0, sprites.Count);
            bloodOverlay.sprite = sprites[index];
            yield return Tools.Fade(bloodOverlay, duration, true, maxFade);
            yield return Tools.Fade(bloodOverlay, duration, false, maxFade);
        }
    }
}
