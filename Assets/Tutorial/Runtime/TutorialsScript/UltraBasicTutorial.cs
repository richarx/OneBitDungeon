using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Enemies.Scripts;
using Enemies.Spawner;
using Game_Manager;
using Player.Scripts;
using Sirenix.OdinInspector;
using UI.Arrogance;
using UnityEngine;

namespace Tutorials
{
    /// <summary>
    /// Local, deliberately linear orchestration for TutorialRoom. The reusable
    /// runner remains responsible for objective validation and presentation.
    /// </summary>
    public sealed class UltraBasicTutorial : MonoBehaviour
    {
        [TitleGroup("References")]
        [SerializeField, Required]
        private TutorialRunner _runner;

        [TitleGroup("References")]
        [SerializeField, Required]
        private TutorialAttackEmitter _attackEmitter;

        [TitleGroup("References")]
        [SerializeField, Required]
        private GameObject _dummy;

        [TitleGroup("Placement")]
        [SerializeField, Required]
        private Transform _playerExercisePoint;

        [TitleGroup("Placement")]
        [SerializeField, Required]
        private Transform _dummyExercisePoint;

        [TitleGroup("Placement")]
        [SerializeField, Required]
        private Transform _dangerStartPoint;

        [TitleGroup("Placement")]
        [SerializeField, Required]
        private Transform _closeDodgePoint;

        [TitleGroup("Steps")]
        [SerializeField, Required]
        private TutorialData _arroganceAndTaunt;

        [TitleGroup("Steps")]
        [SerializeField, Required]
        private TutorialData _criticalAttack;

        [TitleGroup("Steps")]
        [SerializeField, Required]
        private TutorialData _loseArrogance;

        [TitleGroup("Steps")]
        [SerializeField, Required]
        private TutorialData _tauntInDangerZone;

        [TitleGroup("Steps")]
        [SerializeField, Required]
        private TutorialData _spin;

        [TitleGroup("Steps")]
        [SerializeField, Required]
        private TutorialData _closeDodge;

        [TitleGroup("Steps")]
        [SerializeField, Required]
        private TutorialData _parry;

        [TitleGroup("Steps")]
        [SerializeField, Required]
        private TutorialData _jump;

        [TitleGroup("Timing")]
        [SerializeField, MinValue(0.0f)]
        private float _minimumReadingDuration = 0.55f;

        [TitleGroup("Timing")]
        [SerializeField, MinValue(0.0f)]
        private float _attackRetryInterval = 0.8f;

        [TitleGroup("Debug")]
        [SerializeField, Range(1, 8)]
        private int _debugStartStep = 1;

        private CancellationTokenSource _executionCancellation;
        private PlayerStateMachine _player;
        private PlayerHealth _playerHealth;
        private bool _previousInvincibility;
        private bool _hasSavedInvincibility;
        private bool _isExecuting;

        private void OnEnable()
        {
            GameManager.OnChangeScene.RemoveListener(HandleSceneChanged); // wass ?
            GameManager.OnChangeScene.AddListener(HandleSceneChanged);
        }

        private void OnDisable()
        {
            GameManager.OnChangeScene.RemoveListener(HandleSceneChanged);
            StopTutorial();
            RestorePlayerProtection();
        }

        [TitleGroup("Debug")]
        [Button(ButtonSizes.Large)]
        public void LaunchTutorial()
        {
            StartAtStepAsync(0).Forget();
        }

        [TitleGroup("Debug")]
        [Button]
        public void RestartTutorial()
        {
            StartAtStepAsync(0).Forget();
        }

        [TitleGroup("Debug")]
        [Button]
        public void StartAtDebugStep()
        {
            StartAtStepAsync(_debugStartStep - 1).Forget();
        }

        [TitleGroup("Debug")]
        [Button]
        public void StopTutorial()
        {
            _executionCancellation?.Cancel();
            _attackEmitter?.CancelAll();
            SetArroganceHighlight(false);
        }

        private void HandleSceneChanged()
        {
            LaunchTutorial();
        }

        private async UniTaskVoid StartAtStepAsync(int startStep)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning($"[{nameof(UltraBasicTutorial)}] Enter Play Mode before launching the tutorial.", this);
                return;
            }

            StopTutorial();
            await UniTask.WaitUntil(() => !_isExecuting);

            if (!isActiveAndEnabled)
                return;

            _executionCancellation = new CancellationTokenSource();
            RunTutorialAsync(Mathf.Clamp(startStep, 0, 7), _executionCancellation).Forget();
        }

        private async UniTask RunTutorialAsync(int startStep, CancellationTokenSource cancellation)
        {
            _isExecuting = true;

            try
            {
                ResolveAndValidateDependencies();
                SaveAndEnablePlayerProtection();
                PlaceDummy();

                TutorialData[] steps = GetSteps();
                for (int step = startStep; step < steps.Length; step++)
                    await RunStepAsync(step, steps[step], cancellation.Token);

                _attackEmitter.CancelAll();
                _player.playerArrogance.ClearArrogance();
                GameManager.OnUnlockLevel?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // Stopping, restarting, disabling, and changing scene are expected paths.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                _attackEmitter?.CancelAll();
                SetArroganceHighlight(false);
                RestorePlayerProtection();

                if (ReferenceEquals(_executionCancellation, cancellation))
                    _executionCancellation = null;

                cancellation.Dispose();
                _isExecuting = false;
            }
        }

        private async UniTask RunStepAsync(int step, TutorialData data, CancellationToken cancellationToken)
        {
            PrepareStep(step);

            using (CancellationTokenSource exerciseCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                void HandleObjectivesCompleted(TutorialData completedData)
                {
                    if (completedData != data)
                        return;

                    _attackEmitter.CancelAll();
                    exerciseCancellation.Cancel();
                }

                _runner.OnObjectivesCompleted += HandleObjectivesCompleted;
                try
                {
                    UniTask runnerTask = _runner.RunAsync(data, cancellationToken);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    await UniTask.Delay(TimeSpan.FromSeconds(_minimumReadingDuration), cancellationToken: cancellationToken);

                    UniTask exerciseTask = StartExerciseAsync(step, exerciseCancellation.Token);
                    await runnerTask;

                    if (step == 1)
                    {
                        await UniTask.WaitUntil(
                            () => _player.currentBehaviour.GetBehaviourType() != BehaviourType.CriticalAttack,
                            cancellationToken: cancellationToken);
                    }
                    else if (step == 2)
                    {
                        await UniTask.WaitUntil(
                            () => _player.playerArrogance.NormalizedArrogance <= 0.001f,
                            cancellationToken: cancellationToken);
                    }
                    else if (step == 4)
                    {
                        await UniTask.WaitUntil(
                            () => _player.currentBehaviour.GetBehaviourType() != BehaviourType.ArrogantSpin,
                            cancellationToken: cancellationToken);
                    }

                    exerciseCancellation.Cancel();
                    try
                    {
                        await exerciseTask;
                    }
                    catch (OperationCanceledException)
                    {
                        // The runner completed successfully and stopped the active exercise.
                    }
                }
                finally
                {
                    _runner.OnObjectivesCompleted -= HandleObjectivesCompleted;
                    _attackEmitter.CancelAll();
                }
            }
        }

        private UniTask StartExerciseAsync(int step, CancellationToken cancellationToken)
        {
            switch (step)
            {
                case 0:
                case 4:
                    return UniTask.Never(cancellationToken);
                case 1:
                    return RefillArroganceAfterCriticalAttackAsync(cancellationToken);
                case 2:
                    return RepeatAttackAsync(
                        TutorialAttackKind.Demonstration,
                        _playerExercisePoint.position,
                        cancellationToken,
                        refillArroganceBeforeEachAttempt: true);
                case 3:
                    return RepeatAttackAsync(TutorialAttackKind.Demonstration, _dangerStartPoint.position, cancellationToken);
                case 5:
                    return RepeatAttackAsync(TutorialAttackKind.Demonstration, _closeDodgePoint.position, cancellationToken);
                case 6:
                    return WaitForArroganceReleaseThenRepeatAsync(TutorialAttackKind.Parry, cancellationToken);
                case 7:
                    return WaitForArroganceReleaseThenRepeatAsync(TutorialAttackKind.Jump, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException(nameof(step));
            }
        }

        private async UniTask WaitForArroganceReleaseThenRepeatAsync(TutorialAttackKind kind, CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(
                () => !_player.isInArroganceMode && !_player.inputPackage.GetArroganceMode.isPressed,
                cancellationToken: cancellationToken);

            await RepeatAttackAsync(kind, _player.position, cancellationToken);
        }

        private async UniTask RefillArroganceAfterCriticalAttackAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await UniTask.WaitUntil(
                    () => _player.currentBehaviour.GetBehaviourType() == BehaviourType.CriticalAttack,
                    cancellationToken: cancellationToken);

                await UniTask.WaitUntil(
                    () => _player.currentBehaviour.GetBehaviourType() != BehaviourType.CriticalAttack,
                    cancellationToken: cancellationToken);

                _player.playerArrogance.FillArrogance();
            }
        }

        private async UniTask RepeatAttackAsync(
            TutorialAttackKind kind,
            Vector3 fixedTarget,
            CancellationToken cancellationToken,
            bool refillArroganceBeforeEachAttempt = false)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (refillArroganceBeforeEachAttempt)
                    _player.playerArrogance.FillArrogance();

                PlaceDummy();
                _attackEmitter.Launch(kind, _dummyExercisePoint.position, fixedTarget);

                float retryDelay = _attackEmitter.GetDuration(kind) + _attackRetryInterval;
                await UniTask.Delay(TimeSpan.FromSeconds(retryDelay), cancellationToken: cancellationToken);
            }
        }

        private void PrepareStep(int step) //On pourrait pas mettre ces trucs dans les Tutorial Data ? Pour avoir moins de trucs particulier dans ce script
        {
            _attackEmitter.CancelAll();
            SetArroganceHighlight(false);
            PlaceDummy();

            switch (step)
            {
                case 0:
                    PlacePlayer(_playerExercisePoint);
                    _player.playerArrogance.ClearArrogance();
                    _player.arroganceProcessor.ResetTauntDurationTracking();
                    SetArroganceHighlight(true);
                    break;
                case 1:
                    PlacePlayer(_playerExercisePoint);
                    _player.playerArrogance.FillArrogance();
                    break;
                case 2:
                    PlacePlayer(_playerExercisePoint);
                    _player.playerArrogance.FillArrogance();
                    break;
                case 3:
                    PlacePlayer(_dangerStartPoint);
                    _player.playerArrogance.ClearArrogance();
                    _player.arroganceProcessor.ResetTauntDurationTracking();
                    break;
                case 4:
                    PlacePlayer(_playerExercisePoint);
                    break;
                case 5:
                    PlacePlayer(_closeDodgePoint);
                    _player.playerArrogance.ClearArrogance();
                    break;
                case 6:
                    PlacePlayer(_playerExercisePoint);
                    break;
                case 7:
                    PlacePlayer(_playerExercisePoint);
                    break;
            }
        }

        private void ResolveAndValidateDependencies()
        {
            _player = PlayerStateMachine.instance;
            if (_player == null)
                throw new InvalidOperationException("The tutorial needs an active player.");

            _playerHealth = _player.playerHealth;
            if (_runner == null || _attackEmitter == null || _dummy == null || _playerHealth == null)
                throw new InvalidOperationException("Assign the tutorial runner, attack emitter, dummy, and player references.");

            if (_player.playerArrogance == null || _player.arroganceProcessor == null || _player.playerSword == null || !_player.playerSword.CurrentlyHasSword)
                throw new InvalidOperationException("The tutorial player needs an arrogance component, processor, and available sword.");

            if (_player.playerData == null || !_player.playerData.loseAllArroganceOnHit)
                throw new InvalidOperationException("TutorialRoom requires a PlayerData profile that loses all arrogance when hit.");

            Damageable dummyDamageable = _dummy.GetComponent<Damageable>();
            if (dummyDamageable == null || dummyDamageable.IsDead)
                throw new InvalidOperationException("The tutorial dummy must be alive and damageable.");

            if (EnemyHolder.instance == null || EnemyHolder.instance.MainEnemy != _dummy)
                throw new InvalidOperationException("The tutorial dummy must be registered as the main enemy.");

            foreach (TutorialData step in GetSteps())
            {
                if (step == null)
                    throw new InvalidOperationException("Assign all eight tutorial step assets.");
            }
        }

        private TutorialData[] GetSteps()
        {
            return new[]
            {
                _arroganceAndTaunt,
                _criticalAttack,
                _loseArrogance,
                _tauntInDangerZone,
                _spin,
                _closeDodge,
                _parry,
                _jump
            };
        }

        private void SaveAndEnablePlayerProtection()
        {
            _previousInvincibility = _playerHealth.IsInvincible;
            _hasSavedInvincibility = true;
            _playerHealth.SetInvincible(true);
        }

        private void RestorePlayerProtection()
        {
            if (!_hasSavedInvincibility || _playerHealth == null)
                return;

            _playerHealth.SetInvincible(_previousInvincibility);
            _hasSavedInvincibility = false;
        }

        private void PlacePlayer(Transform point)
        {
            if (point != null)
                _player.TeleportPlayer(point.position);
        }

        private void PlaceDummy()
        {
            if (_dummy != null && _dummyExercisePoint != null)
                _dummy.transform.position = _dummyExercisePoint.position;
        }

        private void SetArroganceHighlight(bool visible)
        {
            // _arroganceDisplay?.SetTutorialHighlightVisible(visible);
        }
    }
}
