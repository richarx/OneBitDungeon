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

            GetComponent<Damageable>().OnTakeDamage.AddListener(() =>
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
