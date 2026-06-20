using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TataSequencing
{
    public sealed class Sequencer : MonoBehaviour
    {
        [TitleGroup("Playback")]
        [SerializeField]
        private bool startOnPlay;

        [TitleGroup("Playback")]
        [SerializeField]
        private bool startOnEnable;

        [TitleGroup("Playback")]
        [SerializeField]
        [Min(0.01f)]
        private float speedFactor = 1f;

        [TitleGroup("Actions")]
        [SerializeReference]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(
            DraggableItems = true,
            ShowFoldout = true,
            HideAddButton = false,
            ShowIndexLabels = true,
            NumberOfItemsPerPage = 20,
            ListElementLabelName = nameof(SequencerAction.ListLabel))]
        [TypeFilter(nameof(GetActionTypes))]
        private List<SequencerAction> actions = new();

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        public bool IsPlaying { get; private set; }

        public bool IsStopped => !IsPlaying;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        public int CurrentActionIndex { get; private set; }

        public float SpeedFactor => Mathf.Clamp(speedFactor, 0.01f, 10f);

        private CancellationTokenSource playCancellationToken;
        private UniTaskCompletionSource stopConfirm;
        private SequencerContext context;
        private bool preventAutoRestart;

        private void Awake()
        {
            context = new SequencerContext(this);
        }

        private void Start()
        {
            if (startOnPlay)
            {
                Play();
            }
        }

        private void OnEnable()
        {
            if (startOnEnable && !IsPlaying)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            preventAutoRestart = true;
            Stop();
        }

        private void OnDestroy()
        {
            preventAutoRestart = true;
            Stop();
            playCancellationToken?.Dispose();
            playCancellationToken = null;
        }

        [Button(ButtonSizes.Medium), GUIColor(0.3f, 0.9f, 0.4f)]
        public void Play()
        {
            PlayAsync().Forget();
        }

        [Button(ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
        public void Stop()
        {
            if (!IsPlaying)
            {
                return;
            }

            playCancellationToken?.Cancel();

            foreach (SequencerAction action in actions.Where(action => action != null))
            {
                try
                {
                    action.Cancel(context);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        [Button(ButtonSizes.Medium)]
        public void StopAndPlay()
        {
            StopAndPlayAsync().Forget();
        }

        public async UniTask PlayAsync()
        {
            if (IsPlaying)
            {
                return;
            }

            preventAutoRestart = false;
            CurrentActionIndex = 0;
            context.CurrentLoopAnchorIndex = -1;

            foreach (SequencerAction action in actions.Where(action => action != null))
            {
                action.ResetRuntimeState();
            }

            playCancellationToken?.Dispose();
            playCancellationToken = new CancellationTokenSource();
            stopConfirm = new UniTaskCompletionSource();
            IsPlaying = true;

            try
            {
                while (CurrentActionIndex < actions.Count && !playCancellationToken.IsCancellationRequested)
                {
                    SequencerAction action = actions[CurrentActionIndex];
                    context.CurrentActionIndex = CurrentActionIndex;

                    if (action == null || !action.Enabled)
                    {
                        CurrentActionIndex++;
                        continue;
                    }

                    switch (action)
                    {
                        case LoopAnchorAction:
                            context.CurrentLoopAnchorIndex = CurrentActionIndex;
                            CurrentActionIndex++;
                            break;

                        case LoopAction loopAction:
                            if (context.CurrentLoopAnchorIndex < 0)
                            {
                                Debug.LogWarning($"[{name}] A LoopAction was reached without a previous LoopAnchorAction.", this);
                                CurrentActionIndex++;
                                break;
                            }

                            CurrentActionIndex = loopAction.ShouldJump()
                                ? context.CurrentLoopAnchorIndex
                                : CurrentActionIndex + 1;
                            break;

                        default:
                            await action.ExecuteAsync(context, playCancellationToken.Token);
                            CurrentActionIndex++;
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                IsPlaying = false;
                stopConfirm?.TrySetResult();
            }
        }

        public async UniTask StopAndPlayAsync()
        {
            Stop();

            if (stopConfirm != null)
            {
                await stopConfirm.Task;
            }

            if (!preventAutoRestart && this != null && isActiveAndEnabled)
            {
                await PlayAsync();
            }
        }

        [Button]
        public void ResetPlaybackCursor()
        {
            CurrentActionIndex = 0;
            if (context != null)
            {
                context.CurrentActionIndex = 0;
                context.CurrentLoopAnchorIndex = -1;
            }

            foreach (SequencerAction action in actions.Where(action => action != null))
            {
                action.ResetRuntimeState();
            }
        }

        private IEnumerable<Type> GetActionTypes()
        {
            return typeof(SequencerAction).Assembly
                .GetTypes()
                .Where(type =>
                    typeof(SequencerAction).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsGenericType);
        }
    }
}
