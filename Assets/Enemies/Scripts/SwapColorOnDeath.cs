using System.Collections.Generic;
using Enemies.Scripts;
using UnityEngine;

public class SwapColorOnDeath : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> spriteRenderers;
    [SerializeField] private Color targetColor;

    private void Start()
    {
        GetComponent<Damageable>().OnDie.AddListener(SwapColor);
    }

    private void SwapColor()
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.color = targetColor;
        }
    }
}
