using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tutorials
{
    public sealed class TutorialTestSequencer : MonoBehaviour
    {
        [TitleGroup("References")]
        [SerializeField, Required]
        private TutorialRunner _runner;

        [TitleGroup("Tutorial")]
        [SerializeField, Required]
        private TutorialData _tutorial;

        [TitleGroup("Launch")]
        [SerializeField]
        private bool _launchOnStart = true;

        private CancellationTokenSource _executionCancellation;

        private void Start()
        {
            if (_launchOnStart)
                LaunchTutorial();
        }

        private void OnDisable()
        {
            StopTutorial();
        }

        [TitleGroup("Launch")]
        [Button(ButtonSizes.Large)]
        public void LaunchTutorial()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Tutorial Test Sequencer] Enter Play Mode before launching.", this);
                return;
            }

            if (_runner == null || _tutorial == null)
            {
                Debug.LogError("[Tutorial Test Sequencer] Assign the runner and tutorial before launching.", this);
                return;
            }

            StopTutorial();

            CancellationTokenSource cancellation = new CancellationTokenSource();
            _executionCancellation = cancellation;
            RunTutorialAsync(cancellation).Forget();
        }

        [TitleGroup("Launch")]
        [Button]
        public void StopTutorial()
        {
            CancellationTokenSource cancellation = _executionCancellation;
            _executionCancellation = null;

            if (cancellation != null && !cancellation.IsCancellationRequested)
                cancellation.Cancel();
        }

        private async UniTask RunTutorialAsync(CancellationTokenSource cancellation)
        {
            try
            {
                await _runner.RunAsync(_tutorial, cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected when stopping or disabling the test sequencer.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                if (ReferenceEquals(_executionCancellation, cancellation))
                    _executionCancellation = null;

                cancellation.Dispose();
            }
        }
    }
}
