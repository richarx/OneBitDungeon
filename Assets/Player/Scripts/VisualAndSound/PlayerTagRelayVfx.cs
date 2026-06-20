using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TataSequencing;
using UnityEngine;

namespace Player.Scripts
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Sequencer))]
    public class PlayerTagRelayVfx : MonoBehaviour
    {
        public enum RelayActor
        {
            Outgoing,
            Incoming
        }

        private Sequencer sequencer;
        private GameObject outgoingGhostObject;
        private SpriteRenderer outgoingGhost;
        private Color outgoingOriginalColor;
        private Vector3 outgoingOriginalLocalPosition;
        private SpriteRenderer incoming;
        private Color incomingOriginalColor;
        private Vector3 incomingOriginalLocalPosition;
        private int incomingOriginalSortingOrder;
        private Vector3 backwardWorldDirection;
        private bool hasPreparedTransition;
        private readonly HashSet<Tween> activeTweens = new HashSet<Tween>();

        public bool HasPreparedTransition => hasPreparedTransition;
        public SpriteRenderer OutgoingGhost => outgoingGhost;
        public SpriteRenderer Incoming => incoming;
        public Color IncomingOriginalColor => incomingOriginalColor;
        public Vector3 IncomingOriginalLocalPosition => incomingOriginalLocalPosition;

        private void Awake()
        {
            sequencer = GetComponent<Sequencer>();
        }

        public void Play(
            SpriteRenderer outgoing,
            SpriteRenderer newIncoming,
            Vector2 lookDirection)
        {
            PlayAsync(outgoing, newIncoming, lookDirection).Forget();
        }

        public SpriteRenderer GetActorRenderer(RelayActor actor)
        {
            return actor == RelayActor.Outgoing
                ? outgoingGhost
                : incoming;
        }

        public Vector3 GetActorOriginalLocalPosition(RelayActor actor)
        {
            return actor == RelayActor.Outgoing
                ? outgoingOriginalLocalPosition
                : incomingOriginalLocalPosition;
        }

        public float GetActorOriginalAlpha(RelayActor actor)
        {
            return actor == RelayActor.Outgoing
                ? outgoingOriginalColor.a
                : incomingOriginalColor.a;
        }

        public float GetActorAlpha(RelayActor actor)
        {
            SpriteRenderer renderer = GetActorRenderer(actor);
            return renderer != null ? renderer.color.a : 0.0f;
        }

        public Vector3 GetBackwardLocalOffset(Transform parent, float distance)
        {
            Vector3 worldOffset = backwardWorldDirection * distance;
            return parent != null
                ? parent.InverseTransformVector(worldOffset)
                : worldOffset;
        }

        public void SetActorAlpha(RelayActor actor, float alpha)
        {
            SetAlpha(GetActorRenderer(actor), alpha);
        }

        public void TrackTween(Tween tween)
        {
            if (tween == null || !tween.IsActive())
                return;

            activeTweens.Add(tween);
            _ = tween.OnKill(() => activeTweens.Remove(tween));
        }

        public async UniTask WaitForTrackedTweensAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(
                () => !HasActiveTweens(),
                cancellationToken: cancellationToken);
        }

        public void CompleteTransition()
        {
            KillTrackedTweens();
            GetComponent<AfterImage>()?.Cancel();
            RestoreIncoming();
            DestroyOutgoingGhost();
            hasPreparedTransition = false;
        }

        public void CancelTransition()
        {
            KillTrackedTweens();
            GetComponent<AfterImage>()?.Cancel();
            RestoreIncoming();
            DestroyOutgoingGhost();
            hasPreparedTransition = false;
        }

        private async UniTaskVoid PlayAsync(
            SpriteRenderer outgoing,
            SpriteRenderer newIncoming,
            Vector2 lookDirection)
        {
            if (outgoing == null
                || newIncoming == null
                || outgoing.sprite == null
                || newIncoming.sprite == null)
            {
                return;
            }

            if (sequencer == null)
                sequencer = GetComponent<Sequencer>();

            if (sequencer.IsPlaying)
            {
                sequencer.Stop();
                await UniTask.WaitUntil(
                    () => sequencer == null || sequencer.IsStopped,
                    cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            if (sequencer == null || !isActiveAndEnabled)
                return;

            PrepareTransition(outgoing, newIncoming, lookDirection);
            sequencer.Play();
        }

        private void PrepareTransition(
            SpriteRenderer outgoing,
            SpriteRenderer newIncoming,
            Vector2 lookDirection)
        {
            CancelTransition();

            Vector2 facing = lookDirection.sqrMagnitude > 0.01f
                ? lookDirection.normalized
                : Vector2.right;
            backwardWorldDirection = new Vector3(-facing.x, 0.0f, -facing.y);

            incoming = newIncoming;
            incomingOriginalColor = incoming.color;
            incomingOriginalLocalPosition = incoming.transform.localPosition;
            incomingOriginalSortingOrder = incoming.sortingOrder;
            incoming.sortingOrder = incomingOriginalSortingOrder - 1;
            SetActorAlpha(RelayActor.Incoming, 0.0f);

            outgoingGhost = CreateGhost(outgoing);
            if (outgoingGhost != null)
            {
                outgoingOriginalColor = outgoingGhost.color;
                outgoingOriginalLocalPosition = outgoingGhost.transform.localPosition;
            }
            hasPreparedTransition = outgoingGhost != null;
        }

        private SpriteRenderer CreateGhost(SpriteRenderer source)
        {
            outgoingGhostObject = new GameObject("Tag Outgoing");
            outgoingGhostObject.layer = source.gameObject.layer;

            Transform ghostTransform = outgoingGhostObject.transform;
            ghostTransform.SetParent(source.transform.parent, false);
            ghostTransform.localPosition = source.transform.localPosition;
            ghostTransform.localRotation = source.transform.localRotation;
            ghostTransform.localScale = source.transform.localScale;

            SpriteRenderer ghost = outgoingGhostObject.AddComponent<SpriteRenderer>();
            CopyRenderer(source, ghost);
            ghost.color = source.color;
            return ghost;
        }

        private void RestoreIncoming()
        {
            if (incoming == null)
                return;

            incoming.color = incomingOriginalColor;
            incoming.transform.localPosition = incomingOriginalLocalPosition;
            incoming.sortingOrder = incomingOriginalSortingOrder;
            incoming = null;
        }

        private void DestroyOutgoingGhost()
        {
            if (outgoingGhostObject != null)
                Destroy(outgoingGhostObject);

            outgoingGhostObject = null;
            outgoingGhost = null;
        }

        private bool HasActiveTweens()
        {
            activeTweens.RemoveWhere(tween => tween == null || !tween.IsActive());
            return activeTweens.Count > 0;
        }

        private void KillTrackedTweens()
        {
            if (activeTweens.Count == 0)
                return;

            Tween[] tweens = new Tween[activeTweens.Count];
            activeTweens.CopyTo(tweens);

            for (int i = 0; i < tweens.Length; i++)
            {
                Tween tween = tweens[i];
                if (tween != null && tween.IsActive())
                    tween.Kill(false);
            }

            activeTweens.Clear();
        }

        private static void CopyRenderer(
            SpriteRenderer source,
            SpriteRenderer destination)
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

        private static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            if (renderer == null)
                return;

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }

        private void OnDisable()
        {
            sequencer?.Stop();
            CancelTransition();
        }
    }
}
