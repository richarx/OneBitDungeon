using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Scripts
{
    [DisallowMultipleComponent]
    public class PlayerTagRelayVfx : MonoBehaviour
    {
        [Header("Outgoing Character")]
        [SerializeField, Min(0.01f)] private float outgoingDuration = 0.16f;
        [SerializeField, Min(0.0f)] private float outgoingRetreatDistance = 0.28f;

        [Header("Afterimages")]
        [SerializeField, Range(0, 8)] private int afterimageCount = 3;
        [SerializeField, Min(0.0f)] private float afterimageInterval = 0.035f;
        [SerializeField, Min(0.01f)] private float afterimageLifetime = 0.14f;
        [SerializeField, Range(0.0f, 1.0f)] private float afterimageOpacity = 0.38f;
        [SerializeField, Range(0.5f, 2.0f)] private float afterimageBrightness = 1.25f;

        [Header("Incoming Character")]
        [SerializeField, Min(0.0f)] private float incomingDelay = 0.025f;
        [SerializeField, Min(0.01f)] private float incomingDuration = 0.18f;
        [SerializeField, Min(0.0f)] private float incomingStartDistance = 0.48f;

        private readonly List<GameObject> spawnedObjects = new List<GameObject>();
        private readonly List<Afterimage> afterimages = new List<Afterimage>();

        private Coroutine currentEffect;
        private SpriteRenderer animatedIncoming;
        private Color incomingOriginalColor;
        private Vector3 incomingOriginalPosition;
        private int incomingOriginalSortingOrder;

        private sealed class Afterimage
        {
            public SpriteRenderer renderer;
            public float spawnTime;
            public float initialAlpha;
        }

        public void Play(SpriteRenderer outgoing, SpriteRenderer incoming, Vector2 lookDirection)
        {
            CleanupCurrentEffect();

            if (outgoing == null || incoming == null || outgoing.sprite == null || incoming.sprite == null)
                return;

            Vector2 facing = lookDirection.sqrMagnitude > 0.01f
                ? lookDirection.normalized
                : Vector2.right;
            Vector3 backwardWorldDirection = new Vector3(-facing.x, 0.0f, -facing.y);

            animatedIncoming = incoming;
            incomingOriginalColor = incoming.color;
            incomingOriginalPosition = incoming.transform.localPosition;
            incomingOriginalSortingOrder = incoming.sortingOrder;

            Vector3 incomingOffset = WorldOffsetToLocal(
                incoming.transform.parent,
                backwardWorldDirection * incomingStartDistance);

            incoming.transform.localPosition = incomingOriginalPosition + incomingOffset;
            incoming.sortingOrder = incomingOriginalSortingOrder - 1;
            SetAlpha(incoming, 0.0f);

            currentEffect = StartCoroutine(PlayEffect(
                outgoing,
                incoming,
                backwardWorldDirection));
        }

        private IEnumerator PlayEffect(
            SpriteRenderer outgoing,
            SpriteRenderer incoming,
            Vector3 backwardWorldDirection)
        {
            SpriteRenderer outgoingGhost = CreateGhost(outgoing, "Tag Outgoing");
            Vector3 outgoingStartPosition = outgoingGhost.transform.localPosition;
            Vector3 outgoingOffset = WorldOffsetToLocal(
                outgoingGhost.transform.parent,
                backwardWorldDirection * outgoingRetreatDistance);
            Color outgoingColor = outgoingGhost.color;

            int spawnedAfterimageCount = 0;
            float nextAfterimageTime = afterimageInterval;
            float totalDuration = Mathf.Max(
                outgoingDuration,
                incomingDelay + incomingDuration,
                afterimageCount * afterimageInterval + afterimageLifetime);

            float elapsed = 0.0f;
            while (elapsed < totalDuration)
            {
                float outgoingProgress = Mathf.Clamp01(elapsed / outgoingDuration);
                outgoingGhost.transform.localPosition = Vector3.LerpUnclamped(
                    outgoingStartPosition,
                    outgoingStartPosition + outgoingOffset,
                    EaseOutCubic(outgoingProgress));
                SetAlpha(outgoingGhost, outgoingColor.a * (1.0f - EaseInCubic(outgoingProgress)));

                while (spawnedAfterimageCount < afterimageCount && elapsed >= nextAfterimageTime)
                {
                    SpawnAfterimage(outgoing, outgoingGhost.transform, elapsed);
                    spawnedAfterimageCount++;
                    nextAfterimageTime += afterimageInterval;
                }

                UpdateAfterimages(elapsed);
                UpdateIncoming(incoming, backwardWorldDirection, elapsed);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            RestoreIncoming();
            currentEffect = null;
            DestroySpawnedObjects();
        }

        private void UpdateIncoming(
            SpriteRenderer incoming,
            Vector3 backwardWorldDirection,
            float elapsed)
        {
            if (elapsed < incomingDelay)
                return;

            float progress = Mathf.Clamp01((elapsed - incomingDelay) / incomingDuration);
            Vector3 startOffset = WorldOffsetToLocal(
                incoming.transform.parent,
                backwardWorldDirection * incomingStartDistance);

            incoming.transform.localPosition = Vector3.LerpUnclamped(
                incomingOriginalPosition + startOffset,
                incomingOriginalPosition,
                EaseOutCubic(progress));
            SetAlpha(incoming, incomingOriginalColor.a * SmoothStep(progress));
        }

        private void SpawnAfterimage(
            SpriteRenderer source,
            Transform movingGhost,
            float spawnTime)
        {
            SpriteRenderer afterimage = CreateGhost(source, "Tag Afterimage");
            afterimage.transform.localPosition = movingGhost.localPosition;
            afterimage.transform.localRotation = movingGhost.localRotation;
            afterimage.transform.localScale = movingGhost.localScale;
            afterimage.sortingOrder = source.sortingOrder - 1;

            Color color = source.color;
            color.r *= afterimageBrightness;
            color.g *= afterimageBrightness;
            color.b *= afterimageBrightness;
            color.a *= afterimageOpacity;
            afterimage.color = color;

            afterimages.Add(new Afterimage
            {
                renderer = afterimage,
                spawnTime = spawnTime,
                initialAlpha = color.a
            });
        }

        private void UpdateAfterimages(float elapsed)
        {
            for (int i = afterimages.Count - 1; i >= 0; i--)
            {
                Afterimage afterimage = afterimages[i];
                if (afterimage.renderer == null)
                {
                    afterimages.RemoveAt(i);
                    continue;
                }

                float progress = Mathf.Clamp01((elapsed - afterimage.spawnTime) / afterimageLifetime);
                SetAlpha(afterimage.renderer, afterimage.initialAlpha * (1.0f - EaseOutCubic(progress)));

                if (progress >= 1.0f)
                {
                    afterimage.renderer.enabled = false;
                    afterimages.RemoveAt(i);
                }
            }
        }

        private SpriteRenderer CreateGhost(SpriteRenderer source, string objectName)
        {
            GameObject ghostObject = new GameObject(objectName);
            spawnedObjects.Add(ghostObject);
            ghostObject.layer = source.gameObject.layer;

            Transform ghostTransform = ghostObject.transform;
            ghostTransform.SetParent(source.transform.parent, false);
            ghostTransform.localPosition = source.transform.localPosition;
            ghostTransform.localRotation = source.transform.localRotation;
            ghostTransform.localScale = source.transform.localScale;

            SpriteRenderer ghost = ghostObject.AddComponent<SpriteRenderer>();
            CopyRenderer(source, ghost);
            ghost.color = source.color;
            return ghost;
        }

        private static void CopyRenderer(SpriteRenderer source, SpriteRenderer destination)
        {
            destination.sprite = source.sprite;
            destination.sharedMaterial = source.sharedMaterial;
            destination.flipX = source.flipX;
            destination.flipY = source.flipY;
            destination.drawMode = source.drawMode;
            destination.size = source.size;
            destination.maskInteraction = source.maskInteraction;
            destination.spriteSortPoint = source.spriteSortPoint;
            destination.sortingLayerID = source.sortingLayerID;
            destination.sortingOrder = source.sortingOrder;
        }

        private static Vector3 WorldOffsetToLocal(Transform parent, Vector3 worldOffset)
        {
            return parent != null
                ? parent.InverseTransformVector(worldOffset)
                : worldOffset;
        }

        private static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }

        private static float EaseInCubic(float value)
        {
            return value * value * value;
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1.0f - value;
            return 1.0f - inverse * inverse * inverse;
        }

        private static float SmoothStep(float value)
        {
            return value * value * (3.0f - 2.0f * value);
        }

        private void RestoreIncoming()
        {
            if (animatedIncoming == null)
                return;

            animatedIncoming.color = incomingOriginalColor;
            animatedIncoming.transform.localPosition = incomingOriginalPosition;
            animatedIncoming.sortingOrder = incomingOriginalSortingOrder;
            animatedIncoming = null;
        }

        private void CleanupCurrentEffect()
        {
            if (currentEffect != null)
            {
                StopCoroutine(currentEffect);
                currentEffect = null;
            }

            RestoreIncoming();
            DestroySpawnedObjects();
        }

        private void DestroySpawnedObjects()
        {
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (spawnedObjects[i] != null)
                    Destroy(spawnedObjects[i]);
            }

            spawnedObjects.Clear();
            afterimages.Clear();
        }

        private void OnDisable()
        {
            CleanupCurrentEffect();
        }
    }
}
