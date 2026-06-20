using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Player.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public sealed class TriggerAfterImageAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private AfterImage afterImage;

        [FoldoutGroup("Target")]
        [SerializeField]
        private PlayerTagRelayVfx.RelayActor actor;

        [FoldoutGroup("Timing")]
        [MinValue(0.0f)]
        [SerializeField]
        private float duration = 0.16f;

        public override UniTask ExecuteAsync(
            SequencerContext context,
            CancellationToken cancellationToken)
        {
            PlayerTagRelayVfx relay = context.GameObject.GetComponent<PlayerTagRelayVfx>();
            SpriteRenderer renderer = relay != null
                ? relay.GetActorRenderer(actor)
                : null;

            if (afterImage == null || renderer == null)
                return UniTask.CompletedTask;

            afterImage.Trigger(renderer, ScaleDuration(duration, context));
            return UniTask.CompletedTask;
        }

        public override void Cancel(SequencerContext context)
        {
            afterImage?.Cancel();
        }
    }
}
