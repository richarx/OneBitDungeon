using UnityEngine;

namespace TataSequencing
{
    public sealed class SequencerContext
    {
        public SequencerContext(Sequencer owner)
        {
            Owner = owner;
        }

        public Sequencer Owner { get; }

        public GameObject GameObject => Owner.gameObject;

        public Transform Transform => Owner.transform;

        public float SpeedFactor => Owner.SpeedFactor;

        public int CurrentActionIndex { get; internal set; }

        public int CurrentLoopAnchorIndex { get; internal set; } = -1;
    }
}
