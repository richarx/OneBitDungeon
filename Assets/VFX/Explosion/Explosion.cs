using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private class BlastVfx
    {
        public SpriteRenderer spriteRenderer;
        public int spriteIndex;

        public BlastVfx(SpriteRenderer renderer, int index)
        {
            spriteRenderer = renderer;
            spriteIndex = index;
        }
    }

    [SerializeField] private GameObject blastPrefab;
    [SerializeField] private List<Sprite> blastSprites;

    [Space]
    [SerializeField] private int minBlastCount;
    [SerializeField] private int maxBlastCount;

    [Space]
    [SerializeField] private float minRange;
    [SerializeField] private float maxRange;

    [Space]
    [SerializeField] private float timeBetweenUpdates;

    private float lastUpdateTimestamp;

    private List<BlastVfx> blasts = new List<BlastVfx>();
    private void Start()
    {
        int blastCount = Random.Range(minBlastCount, maxBlastCount);

        for (int i = 0; i < blastCount; i++)
        {
            SpawnBlast();
        }

        lastUpdateTimestamp = Time.time;
    }

    private void Update()
    {
        if (Time.time - lastUpdateTimestamp >= timeBetweenUpdates)
            UpdateBlasts();
    }

    private void UpdateBlasts()
    {
        int lastIndex = blastSprites.Count - 1;

        for (int i = blasts.Count - 1; i >= 0; i--)
        {
            BlastVfx blast = blasts[i];
            if (blast.spriteIndex >= lastIndex)
            {
                Destroy(blast.spriteRenderer.transform.parent.gameObject);
                blasts.RemoveAt(i);
            }
            else
            {
                blast.spriteIndex += 1;
                blast.spriteRenderer.sprite = blastSprites[blast.spriteIndex];
            }
        }

        lastUpdateTimestamp = Time.time;
    }

    private void SpawnBlast()
    {
        GameObject blastRoot = Instantiate(blastPrefab, ComputePosition(), Quaternion.identity);
        SpriteRenderer spriteRenderer = blastRoot.transform.GetChild(0).GetComponent<SpriteRenderer>();

        int spriteIndex = Random.Range(0, 6);
        spriteRenderer.sprite = blastSprites[spriteIndex];
        blasts.Add(new BlastVfx(spriteRenderer, spriteIndex));
    }

    private Vector3 ComputePosition()
    {
        return transform.position + Random.insideUnitSphere * Random.Range(minRange, maxRange);
    }
}
