using System.Collections.Generic;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class RockDebris : MonoBehaviour
{
    [SerializeField] private List<Sprite> debrisSprites;

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            SpriteRenderer stone = transform.GetChild(i).GetComponent<SpriteRenderer>();
            stone.sprite = debrisSprites[Random.Range(0, debrisSprites.Count)];
            AnimateStone(stone);
        }

        Destroy(gameObject, 1.5f);
    }

    private void AnimateStone(SpriteRenderer stone)
    {
        Vector3 direction = stone.transform.localPosition.normalized.ToVector2().AddRandomAngleToDirection(-10.0f, 10.0f).ToVector3().normalized;
        float distance = Random.Range(1.5f, 3.5f);
        float height = Random.Range(1.0f, 2.5f);
        Vector3 targetPosition = stone.transform.localPosition + direction * distance;
        Vector3 midPosition = stone.transform.localPosition + direction * (distance / 2.0f) + Vector3.up * height;

        Sequence.Create()
            .Chain(Tween.LocalPosition(stone.transform, midPosition, 0.15f, Ease.OutQuad))
            .Chain(Tween.LocalPosition(stone.transform, targetPosition, 0.15f, Ease.InQuad))
            .Group(Tween.Alpha(stone, 0.0f, 0.15f));
    }
}
