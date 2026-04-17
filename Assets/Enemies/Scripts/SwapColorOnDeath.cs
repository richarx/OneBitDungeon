using Enemies.Scripts;
using UnityEngine;

public class SwapColorOnDeath : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color targetColor;

    private void Start()
    {
        GetComponent<Damageable>().OnDie.AddListener(() =>
        {
            spriteRenderer.color = targetColor;
        });
    }
}
