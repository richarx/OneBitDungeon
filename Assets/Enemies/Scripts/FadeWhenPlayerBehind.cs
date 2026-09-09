using System.Collections.Generic;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Enemies.Scripts
{
    public class FadeWhenPlayerBehind : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private SpriteRenderer occlusionRenderer;
        [SerializeField] private List<SpriteRenderer> renderersToFade = new List<SpriteRenderer>();

        [Title("Fade")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float targetAlpha = 0.35f;
        [Min(0.0f)]
        [SerializeField] private float fadeDuration = 0.15f;

        [SerializeField] private Ease ease = Ease.InOutSine;

        [Title("Occlusion")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float minimumHorizontalCoverage = 0.5f;
        [Range(0.0f, 0.5f)]
        [SerializeField] private float horizontalCoverageHysteresis = 0.05f;

        private readonly Dictionary<SpriteRenderer, float> originalAlphas = new Dictionary<SpriteRenderer, float>();

        private SpriteRenderer playerSpriteRenderer;
        private Damageable damageable;
        private Sequence fadeSequence;
        private bool isFaded;
        private bool ownerIsDead;

        private void Awake()
        {
            CacheOriginalAlphas();
        }

        private void OnEnable()
        {
            damageable = GetComponent<Damageable>();

            if (damageable != null)
                damageable.OnDie.AddListener(HandleOwnerDeath);

            if (PlayerStateMachine.instance != null)
                playerSpriteRenderer = PlayerStateMachine.instance.graphics.GetComponent<SpriteRenderer>();

            if (originalAlphas.Count == 0)
                CacheOriginalAlphas();
        }

        private void LateUpdate()
        {
            if (damageable != null && damageable.IsDead)
            {
                return;
            }

            bool shouldFade = IsPlayerCovered();
            if (shouldFade != isFaded)
                FadeTo(shouldFade);
        }

        private bool IsPlayerCovered()
        {

            if (occlusionRenderer == null || PlayerStateMachine.instance == null || CamerasHolder.instance.mainCamera == null)
                return false;

            if (PlayerStateMachine.instance.playerHealth != null && PlayerStateMachine.instance.playerHealth.IsDead)
                return false;

            if (!TryGetViewportRect(occlusionRenderer.bounds, out Rect occluderRect, out float occluderDepth))
                return false;

            if (playerSpriteRenderer != null && TryGetViewportRect(playerSpriteRenderer.bounds, out Rect playerRect, out float playerDepth))
                return playerDepth > occluderDepth && HasEnoughScreenCoverage(playerRect, occluderRect);

            Vector3 playerViewportPoint = CamerasHolder.instance.mainCamera.WorldToViewportPoint(PlayerStateMachine.instance.transform.position);
            return playerViewportPoint.z > occluderDepth && occluderRect.Contains(playerViewportPoint);
        }

        private bool HasEnoughScreenCoverage(Rect playerRect, Rect occluderRect)
        {
            float verticalOverlap = Mathf.Min(playerRect.yMax, occluderRect.yMax)
                - Mathf.Max(playerRect.yMin, occluderRect.yMin);
            if (verticalOverlap <= 0.0f || playerRect.width <= Mathf.Epsilon)
                return false;

            float horizontalOverlap = Mathf.Min(playerRect.xMax, occluderRect.xMax)
                - Mathf.Max(playerRect.xMin, occluderRect.xMin);
            float horizontalCoverage = Mathf.Max(0.0f, horizontalOverlap) / playerRect.width;
            float requiredCoverage = isFaded
                ? Mathf.Max(0.0f, minimumHorizontalCoverage - horizontalCoverageHysteresis)
                : minimumHorizontalCoverage;

            return horizontalCoverage >= requiredCoverage;
        }

        private bool TryGetViewportRect(Bounds bounds, out Rect viewportRect, out float depth)
        {
            Vector3 center = CamerasHolder.instance.mainCamera.WorldToViewportPoint(bounds.center);
            depth = center.z;
            viewportRect = default;

            if (depth <= 0.0f)
                return false;

            Vector3 extents = bounds.extents;
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            bool hasVisibleCorner = false;

            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 corner = bounds.center + Vector3.Scale(extents, new Vector3(x, y, z));
                        Vector3 viewportPoint = CamerasHolder.instance.mainCamera.WorldToViewportPoint(corner);
                        if (viewportPoint.z <= 0.0f)
                            continue;

                        hasVisibleCorner = true;
                        minX = Mathf.Min(minX, viewportPoint.x);
                        minY = Mathf.Min(minY, viewportPoint.y);
                        maxX = Mathf.Max(maxX, viewportPoint.x);
                        maxY = Mathf.Max(maxY, viewportPoint.y);
                    }

            if (!hasVisibleCorner)
                return false;

            viewportRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return viewportRect.width > 0.0f && viewportRect.height > 0.0f;
        }

        private void FadeTo(bool fadeOut)
        {
            StopFadeTween();
            isFaded = fadeOut;

            if (fadeDuration <= 0.0f)
            {
                SetAlphasImmediately(fadeOut);
                return;
            }

            bool hasRendererToFade = false;
            foreach (KeyValuePair<SpriteRenderer, float> rendererAlpha in originalAlphas)
            {
                if (rendererAlpha.Key != null)
                {
                    hasRendererToFade = true;
                    break;
                }
            }

            if (!hasRendererToFade)
                return;

            Sequence sequence = Sequence.Create();

            foreach (KeyValuePair<SpriteRenderer, float> rendererAlpha in originalAlphas)
            {
                SpriteRenderer spriteRenderer = rendererAlpha.Key;
                if (spriteRenderer == null)
                    continue;

                float alpha = fadeOut ? targetAlpha : rendererAlpha.Value;
                sequence.Group(Tween.Alpha(spriteRenderer, alpha, fadeDuration, ease));
            }

            fadeSequence = sequence;
        }

        private void HandleOwnerDeath()
        {
            if (ownerIsDead)
                return;

            ownerIsDead = true;
            StopFadeTween();
        }

        private void StopFadeTween()
        {
            if (fadeSequence.isAlive)
                fadeSequence.Stop();

            fadeSequence = default;
        }

        private void SetAlphasImmediately(bool fadeOut)
        {
            foreach (KeyValuePair<SpriteRenderer, float> rendererAlpha in originalAlphas)
            {
                if (rendererAlpha.Key == null)
                    continue;

                Color color = rendererAlpha.Key.color;
                color.a = fadeOut ? targetAlpha : rendererAlpha.Value;
                rendererAlpha.Key.color = color;
            }
        }

        private void CacheOriginalAlphas()
        {
            originalAlphas.Clear();

            foreach (SpriteRenderer spriteRenderer in renderersToFade)
            {
                if (spriteRenderer != null && !originalAlphas.ContainsKey(spriteRenderer))
                    originalAlphas.Add(spriteRenderer, spriteRenderer.color.a);
            }
        }

        private void OnDisable()
        {
            if (damageable != null)
                damageable.OnDie.RemoveListener(HandleOwnerDeath);

            StopFadeTween();

            if (!ownerIsDead)
            {
                isFaded = false;
                SetAlphasImmediately(false);
            }
        }

        private void OnDestroy()
        {
            StopFadeTween();

            if (!ownerIsDead)
                SetAlphasImmediately(false);
        }
    }
}
