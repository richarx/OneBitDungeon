using System.Collections;
using Tools_and_Scripts;
using UnityEngine;

namespace Enemies.Goon
{
    public class FadeCorpseColor : MonoBehaviour
    {
        [SerializeField] private float duration;
        [SerializeField] private Color targetColor;
        
        private SpriteRenderer spriteRenderer;
        
        private IEnumerator Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            Color currentColor = spriteRenderer.color;

            float timer = 0.0f;
            while (timer <= duration)
            {
                currentColor = Color.Lerp(currentColor, targetColor, Tools.NormalizeValue(timer, 0.0f, duration));
                spriteRenderer.color = currentColor;
                yield return null;
                timer += Time.deltaTime;
            }
        }
    }
}
