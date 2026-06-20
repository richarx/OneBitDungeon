using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    [Serializable]
    public sealed class PlayAudioAction : SequencerAction
    {
        [FoldoutGroup("Target")]
        [Required]
        [SerializeField]
        private AudioSource targetAudioSource;

        [FoldoutGroup("Audio")]
        [ListDrawerSettings(ShowFoldout = true)]
        [SerializeField]
        private List<AudioClip> clips = new();

        [FoldoutGroup("Audio")]
        [SerializeField]
        private bool avoidImmediateRepeat = true;

        [FoldoutGroup("Audio")]
        [SerializeField]
        private bool stopCurrentClipBeforePlay = true;

        [FoldoutGroup("Audio")]
        [SerializeField]
        private bool forceDisableLoopWhilePlaying = true;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private bool waitAction = false;

        [FoldoutGroup("Timing")]
        [SerializeField]
        private bool stopOnCancel = true;

        [NonSerialized]
        private AudioSource activeSource;

        [NonSerialized]
        private AudioClip activeClip;

        [NonSerialized]
        private int activePlaybackId;

        [NonSerialized]
        private int lastPlayedClipIndex = -1;

        [NonSerialized]
        private bool originalLoopState;

        [NonSerialized]
        private bool hasLoopStateToRestore;

        public override async UniTask ExecuteAsync(SequencerContext context, CancellationToken cancellationToken)
        {
            if (targetAudioSource == null)
            {
                Debug.LogWarning($"[{context.Owner.name}] PlayAudioAction has no AudioSource target.", context.Owner);
                return;
            }

            if (!TryGetRandomClip(out AudioClip selectedClip, out int selectedClipIndex))
            {
                Debug.LogWarning($"[{context.Owner.name}] PlayAudioAction requires at least one valid AudioClip.", context.Owner);
                return;
            }

            int playbackId = ++activePlaybackId;
            activeSource = targetAudioSource;
            activeClip = selectedClip;
            lastPlayedClipIndex = selectedClipIndex;

            PrepareAudioSourceForPlayback();

            if (stopCurrentClipBeforePlay && targetAudioSource.isPlaying)
            {
                targetAudioSource.Stop();
            }

            targetAudioSource.clip = selectedClip;
            targetAudioSource.Play();

            if (!waitAction)
            {
                RestoreLoopStateWhenPlaybackEndsAsync(playbackId, targetAudioSource, selectedClip).Forget();
                return;
            }

            try
            {
                await UniTask.WaitUntil(
                    () => playbackId != activePlaybackId ||
                          targetAudioSource == null ||
                          !targetAudioSource.isPlaying ||
                          targetAudioSource.clip != selectedClip,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                if (playbackId == activePlaybackId)
                {
                    activeSource = null;
                    activeClip = null;
                    RestoreLoopState();
                }
            }
        }

        public override void Cancel(SequencerContext context)
        {
            activePlaybackId++;

            if (stopOnCancel && activeSource != null)
            {
                activeSource.Stop();
            }

            activeSource = null;
            activeClip = null;
            RestoreLoopState();
        }

        private void PrepareAudioSourceForPlayback()
        {
            if (!forceDisableLoopWhilePlaying || targetAudioSource == null)
            {
                return;
            }

            originalLoopState = targetAudioSource.loop;
            hasLoopStateToRestore = true;
            targetAudioSource.loop = false;
        }

        private void RestoreLoopState()
        {
            if (!hasLoopStateToRestore || targetAudioSource == null)
            {
                return;
            }

            targetAudioSource.loop = originalLoopState;
            hasLoopStateToRestore = false;
        }

        private async UniTaskVoid RestoreLoopStateWhenPlaybackEndsAsync(int playbackId, AudioSource source, AudioClip clip)
        {
            try
            {
                await UniTask.WaitUntil(
                    () => playbackId != activePlaybackId ||
                          source == null ||
                          !source.isPlaying ||
                          source.clip != clip);
            }
            finally
            {
                if (playbackId == activePlaybackId)
                {
                    activeSource = null;
                    activeClip = null;
                    RestoreLoopState();
                }
            }
        }

        private bool TryGetRandomClip(out AudioClip selectedClip, out int selectedClipIndex)
        {
            selectedClip = null;
            selectedClipIndex = -1;

            if (clips == null || clips.Count == 0)
            {
                return false;
            }

            int validClipCount = 0;
            bool canExcludeLastPlayed = false;

            for (int index = 0; index < clips.Count; index++)
            {
                if (clips[index] == null)
                {
                    continue;
                }

                validClipCount++;
                if (index == lastPlayedClipIndex)
                {
                    canExcludeLastPlayed = true;
                }
            }

            if (validClipCount == 0)
            {
                return false;
            }

            bool excludeLastPlayed = avoidImmediateRepeat && canExcludeLastPlayed && validClipCount > 1;
            int eligibleClipCount = excludeLastPlayed ? validClipCount - 1 : validClipCount;
            int randomOrder = UnityEngine.Random.Range(0, eligibleClipCount);
            int currentOrder = 0;

            for (int index = 0; index < clips.Count; index++)
            {
                AudioClip clip = clips[index];
                if (clip == null)
                {
                    continue;
                }

                if (excludeLastPlayed && index == lastPlayedClipIndex)
                {
                    continue;
                }

                if (currentOrder == randomOrder)
                {
                    selectedClip = clip;
                    selectedClipIndex = index;
                    return true;
                }

                currentOrder++;
            }

            return false;
        }
    }
}
