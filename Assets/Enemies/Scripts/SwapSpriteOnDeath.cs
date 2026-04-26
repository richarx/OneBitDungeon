using Enemies.Scripts;
using UnityEngine;

public class SwapSpriteOnDeath : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Color targetColor;

    private void Start()
    {
        GetComponent<Damageable>().OnDie.AddListener(SwapSprite);
    }

    private void SwapSprite()
    {
        spriteRenderer.color = targetColor;
        spriteRenderer.sprite = sprite;
    }
}
