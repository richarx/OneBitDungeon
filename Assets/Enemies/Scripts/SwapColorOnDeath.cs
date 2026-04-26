using System.Collections.Generic;
using Enemies.Scripts;
using PrimeTween;
using UnityEngine;

public class SwapColorOnDeath : MonoBehaviour
{
    [SerializeField] private SpriteRenderer mainSprite;
    [SerializeField] private SpriteRenderer secondary;
    [SerializeField] private Color targetColor;
    [SerializeField] private Color secondaryTargetColor;
    [SerializeField] private float delay;

    private void Start()
    {
        GetComponent<Damageable>().OnDie.AddListener(SwapColor);
    }

    private void SwapColor()
    {
        if (secondary != null)
        {
            Sequence.Create()
            .ChainDelay(delay)
            .Chain(Tween.Color(mainSprite, targetColor, 0.5f, Ease.OutSine))
            .Group(Tween.Color(secondary, secondaryTargetColor, 0.5f, Ease.OutSine));
        }
        else
        {
            Sequence.Create()
            .ChainDelay(delay)
            .Chain(Tween.Color(mainSprite, targetColor, 0.5f, Ease.OutSine));
        }
    }
}
