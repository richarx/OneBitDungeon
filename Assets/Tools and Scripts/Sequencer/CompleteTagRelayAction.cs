using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Player.Scripts;

namespace TataSequencing
{
    [Serializable]
    public sealed class CompleteTagRelayAction : SequencerAction
    {
        public override async UniTask ExecuteAsync(
            SequencerContext context,
            CancellationToken cancellationToken)
        {
            PlayerTagRelayVfx relay = context.GameObject.GetComponent<PlayerTagRelayVfx>();
            if (relay == null)
                return;

            await relay.WaitForTrackedTweensAsync(cancellationToken);

            if (relay != null && relay.HasPreparedTransition)
                relay.CompleteTransition();
        }

        public override void Cancel(SequencerContext context)
        {
            context.GameObject
                .GetComponent<PlayerTagRelayVfx>()
                ?.CancelTransition();
        }
    }
}
