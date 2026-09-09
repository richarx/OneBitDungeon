using System.Collections;
using UnityEngine;

namespace Enemies.Scripts
{
    public class SwapColorOnHit : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color targetColor;
        [SerializeField] private float duration;

        private Color startingColor;

        private void Start()
        {
            startingColor = spriteRenderer.color;

            GetComponent<Damageable>().OnTakeDamage.AddListener((_) =>
            {
                StopAllCoroutines();
                StartCoroutine(SwapColors());
            });
        }

        private IEnumerator SwapColors()
        {
            Color currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(targetColor.r, targetColor.g, targetColor.b, currentColor.a);
            yield return new WaitForSecondsRealtime(duration);
            currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(startingColor.r, startingColor.g, startingColor.b, currentColor.a);
        }
    }
}
