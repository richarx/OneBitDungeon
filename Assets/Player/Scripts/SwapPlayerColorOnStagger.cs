using System.Collections;
using UnityEngine;

namespace Player.Scripts
{
    public class SwapPlayerColorOnStagger : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color targetColor;
        [SerializeField] private float duration;
        
        private PlayerHealth playerHealth;
        private Color startingColor;

        private void Start()
        {
            startingColor = spriteRenderer.color;

            playerHealth = GetComponent<PlayerHealth>();
            playerHealth.OnPlayerTakeDamage.AddListener((_) =>
            {
                StopAllCoroutines();
                StartCoroutine(SwapColors());
            });
        }

        private IEnumerator SwapColors()
        {
            spriteRenderer.color = targetColor;
            yield return new WaitForSeconds(duration);
            spriteRenderer.color = startingColor;
        }
    }
}
