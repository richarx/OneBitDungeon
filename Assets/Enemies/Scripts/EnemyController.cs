using System;
using System.Collections.Generic;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using Enemies.Spawner;
using Game_Manager;
using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class EnemyController : SerializedMonoBehaviour
{
    // Exposed properties
    [TitleGroup("Phases")]
    [OdinSerialize]
    [OnValueChanged(nameof(BindPhaseOwners))]
    [ListDrawerSettings(ShowFoldout = true)]
    [LabelText("Phases")]
    private List<OdinEnemyPhase> phases = new List<OdinEnemyPhase>();
    public List<OdinEnemyPhase> Phases => phases;

    [TitleGroup("Mort")]
    [OdinSerialize]
    [LabelText("Death Behaviour")]
    [HideReferenceObjectPicker]
    [TypeFilter(nameof(GetInlineBehaviourTypes))]
    private IEnemyBehaviour deathBehaviour;

    [field: SerializeField] public SpriteRenderer Sprite { get; private set; }
    [field: SerializeField] public SpriteRenderer shadowSprite { get; private set; }


    [TitleGroup("Humility")]
    [OdinSerialize]
    [LabelText("Animation")]
    private string humilityAnimation;

    // Runtime Components and state
    public Animator animator { get; private set; }
    public Damageable damageable { get; private set; }
    public EnemyHumility humility { get; private set; }
    public AfterImage afterImage { get; private set; }
    private SphereCollider sphereCollider;

    [TitleGroup("Sélection IA")]
    [SerializeField]
    [MinValue(0.05f)]
    [LabelText("Retry interval without candidate")]
    [SuffixLabel("seconds")]
    private float _emptySelectionRetryInterval = 0.25f;

    [TitleGroup("Sélection IA")]
    [SerializeField]
    [MinValue(0.0f)]
    [LabelText("Exchange memory duration")]
    [SuffixLabel("seconds")]
    private float _exchangeMemoryDuration = 1.0f;

    [TitleGroup("Debug")]
    [ShowInInspector]
    [ReadOnly]
    [LabelText("Context")]
    private EnemyContext _context;

    public EnemyContext Context => _context;

    private bool isDead;
    private BehaviourExecution activeExecution;
    private int executionId;
    private float rootedUntilUnscaledTime;
    private float _nextEmptySelectionRetryUnscaledTime;
    private bool _isWaitingForCandidate;
    private bool _isProcessingSelection;
    private bool _hasPendingSelection;
    private IEnemyBehaviour _pendingCompletedBehaviour;
    private bool _pendingWasTransition;
    private bool _isProcessingDebugExecution;
    private bool _hasPendingDebugExecution;
    private Transform _contextTarget;
    private const int _maxImmediateSelectionsPerRequest = 32;



    // Behaviour and phase state

    [NonSerialized] public UnityEvent OnChangeBehaviour = new UnityEvent();

    private List<IEnemyBehaviour> enemyBehaviours;
    private Queue<IEnemyBehaviour> enemyBehaviourQueue = new Queue<IEnemyBehaviour>();
    public IEnemyBehaviour currentBehaviour { get; private set; }
    public IEnemyBehaviour startingBehaviour { get; private set; }
    public IEnemyBehaviour phaseTransitionBehaviour { get; private set; }

    public int currentPhase { get; private set; } = 0;
    private bool isLastPhase => currentPhase >= GetPhaseCount() - 1;

    // DEBUG

    [TitleGroup("Debug")]
    [SerializeField]
    [LabelText("Debug Mode")]
    private bool debugMode = false;

    public bool DebugMode => debugMode;

    [ShowIf(nameof(debugMode))]
    [TitleGroup("Debug")]
    [OdinSerialize]
    [LabelText("Debug Behaviour")]
    [HideReferenceObjectPicker]
    [TypeFilter(nameof(GetInlineBehaviourTypes))]
    private IEnemyBehaviour debugBehaviour;

    public UnityEvent OnSpawnBoss = new UnityEvent();
    public UnityEvent OnKillBoss = new UnityEvent();
    private bool hasSpawned = false;

    protected virtual void Start()
    {
        BindPhaseOwners();

        ResetRuntimeState();
        animator = Sprite != null ? Sprite.GetComponent<Animator>() : null;
        sphereCollider = GetComponent<SphereCollider>();
        damageable = GetComponent<Damageable>();
        humility = GetComponent<EnemyHumility>();
        afterImage = GetComponent<AfterImage>();
        _context = new EnemyContext();
        _context.SetTarget(_contextTarget != null ? _contextTarget
            : PlayerStateMachine.instance != null ? PlayerStateMachine.instance.transform : null);
        _context.Reset(currentPhase);

        if (damageable == null || humility == null)
        {
            Debug.LogError($"[{name}] Damageable and EnemyHumility components are required.", this);
            return;
        }

        if (debugMode)
        {
            damageable.IsInvincible = true;
            if (EnemyHolder.instance != null)
                EnemyHolder.instance.RegisterEnemy(gameObject, true);
            return;
        }

        if (GetPhaseCount() == 0)
        {
            Debug.LogError($"[{name}] No enemy phase is configured.", this);
            return;
        }

        humility.ResetHumility(phases[currentPhase].maxHumility);
        damageable.ResetHealth(phases[currentPhase].healthPoints);
        EnemyHumility.OnFullHumility.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(humilityAnimation) && animator != null)
                animator.Play(humilityAnimation);
            enemyBehaviourQueue.Clear();
            InterruptCurrentBehaviour();
        });

        damageable.OnDie.AddListener(() =>
        {
            if (!isLastPhase && currentBehaviour != startingBehaviour && damageable.currentHealth <= 0)
            {
                Debug.Log("Trigger Next Phase !");
                currentPhase += 1;
                _context.SetCurrentPhase(currentPhase);
                damageable.ResetHealth(phases[currentPhase].healthPoints);
                humility.ResetHumility(phases[currentPhase].maxHumility);
                enemyBehaviours = GetPhaseBehaviours(currentPhase);
                enemyBehaviourQueue.Clear();
                InterruptCurrentBehaviour();
                startingBehaviour = GetPhaseTransitionBehaviour(currentPhase);
                if (startingBehaviour != null)
                    ExecuteBehaviour(startingBehaviour);
                else
                    RequestNextBehaviourSelection(null, true);
            }
            else
            {
                if (isDead)
                    return;

                isDead = true;
                ClearRoot();
                InterruptCurrentBehaviour();

                IEnemyBehaviour configuredDeathBehaviour = GetDeathBehaviour();
                if (configuredDeathBehaviour != null)
                {
                    ExecuteBehaviour(configuredDeathBehaviour);
                    return;
                }

                HandleMissingDeathBehaviour();
                OnKillBoss?.Invoke();
            }
        });

        enemyBehaviours = GetPhaseBehaviours(currentPhase);
        startingBehaviour = GetPhaseTransitionBehaviour(currentPhase);
        if (startingBehaviour != null)
            ExecuteBehaviour(startingBehaviour);
        else
            RequestNextBehaviourSelection(null, true);
    }

    private void ResetRuntimeState()
    {
        isDead = false;
        currentPhase = 0;
        currentBehaviour = null;
        startingBehaviour = null;
        phaseTransitionBehaviour = null;
        enemyBehaviours = null;
        activeExecution = null;
        executionId = 0;
        _nextEmptySelectionRetryUnscaledTime = 0.0f;
        _isWaitingForCandidate = false;
        _isProcessingSelection = false;
        _hasPendingSelection = false;
        _pendingCompletedBehaviour = null;
        _pendingWasTransition = false;
        _isProcessingDebugExecution = false;
        _hasPendingDebugExecution = false;
        ClearRoot();

        if (_context != null)
            _context.Reset(currentPhase);
    }

    /// <summary>
    /// True while a critical hit prevents this enemy from moving. The duration is measured in unscaled time.
    /// </summary>
    public bool IsRooted => !isDead && Time.unscaledTime < rootedUntilUnscaledTime;

    /// <summary>
    /// Movement behaviours must query this before changing the boss position.
    /// Spawn and phase-transition behaviours intentionally do not use this gate so their scripted placement remains reliable.
    /// </summary>
    public bool CanMove => !isDead && !IsRooted;

    public void ApplyRoot(float duration)
    {
        if (isDead || duration <= 0.0f)
            return;

        rootedUntilUnscaledTime = Mathf.Max(rootedUntilUnscaledTime, Time.unscaledTime + duration);
    }

    private void ClearRoot()
    {
        rootedUntilUnscaledTime = 0.0f;
    }

    private void InterruptCurrentBehaviour()
    {
        IEnemyBehaviour interruptedBehaviour = currentBehaviour;
        RemoveCurrentBehaviourAndExecution();

        if (interruptedBehaviour != null)
            interruptedBehaviour.CancelBehaviour(this);
    }

    private IEnemyBehaviour GetDeathBehaviour()
    {
        if (deathBehaviour == null)
            Debug.LogWarning($"[{name}] No death behaviour is configured; applying the safe death fallback.", this);

        return deathBehaviour;
    }

    private void HandleMissingDeathBehaviour()
    {
        currentBehaviour = null;
        DeactivateHitbox();


        if (Sprite != null && shadowSprite != null)
        {

            Sequence deathSequence = Sequence.Create()
                .Chain(Tween.Color(Sprite, Color.clear, 0.5f))
                .Group(Tween.Color(shadowSprite, Color.clear, 0.5f))
                .ChainCallback(() => { Sprite.enabled = false; shadowSprite.enabled = false; });
        }

        Debug.LogWarning($"[{name}] Applied the safe death fallback and unlocked the level.", this);
        GameManager.OnUnlockLevel?.Invoke();
    }

    protected virtual void Update()
    {
        if (currentBehaviour != null)
        {
            currentBehaviour.UpdateBehaviour(this);
            return;
        }

        TryRetrySelectionAfterWait();
    }

    protected virtual void FixedUpdate()
    {
        if (currentBehaviour != null)
            currentBehaviour.FixedUpdateBehaviour(this);
    }

    private void ExecuteBehaviour(IEnemyBehaviour newBehaviour)
    {
        if (newBehaviour == null)
            return;

        currentBehaviour = newBehaviour;
        _isWaitingForCandidate = false;
        _context?.SetCurrentBehaviour(currentBehaviour);
        BehaviourExecution execution = new BehaviourExecution(this, currentBehaviour, ++executionId);
        activeExecution = execution;
        currentBehaviour.SetSubBehaviourState(false);
        currentBehaviour.StartBehaviour(this, activeExecution);

        // condition uniquement si le comportement se Complete dans le StartBehaviour, ce qui est le cas du DummyImmediateTransitionBehaviour
        if (activeExecution == execution)
            OnChangeBehaviour?.Invoke();

    }

    public void TryCompleteBehaviour(BehaviourExecution execution)
    {
        if (!IsExecutionActive(execution))
        {
            Debug.LogWarning("[" + name + "] Attempted to complete a behaviour execution that is not active.", this);
            return;
        }

        IEnemyBehaviour completedBehaviour = currentBehaviour;
        bool wasTransition = completedBehaviour == startingBehaviour;

        RemoveCurrentBehaviourAndExecution();
        completedBehaviour.StopBehaviour(this);

        if (!debugMode)
            RequestNextBehaviourSelection(completedBehaviour, wasTransition);
        else
            RequestDebugBehaviourExecution();

    }

    public bool IsExecutionActive(BehaviourExecution execution)
    {
        return !isDead
               && execution != null
               && activeExecution == execution;
    }

    private void RemoveCurrentBehaviourAndExecution()
    {
        activeExecution = null;
        currentBehaviour = null;
        _context?.SetCurrentBehaviour(null);
    }

    public void EnqueueBehaviour(IEnemyBehaviour behaviour)
    {
        if (behaviour != null)
            enemyBehaviourQueue.Enqueue(behaviour);
    }

    /// <summary>
    /// Optional passive exchange memory for callers that already own an attributable result.
    /// Selection does not record or react to this data automatically.
    /// </summary>
    public void RecordExchangeResult(EnemyExchangeResult result)
    {
        _context?.RecordExchange(result);
    }

    /// <summary>
    /// Allows a spawner to provide a late-created target without any per-frame lookup.
    /// </summary>
    public void SetContextTarget(Transform target)
    {
        _contextTarget = target;
        _context?.SetTarget(target);
    }

    private void RequestNextBehaviourSelection(IEnemyBehaviour completedBehaviour, bool wasTransition)
    {
        if (_isProcessingSelection)
        {
            _pendingCompletedBehaviour = completedBehaviour;
            _pendingWasTransition = wasTransition;
            _hasPendingSelection = true;
            return;
        }

        _isProcessingSelection = true;
        try
        {
            IEnemyBehaviour nextCompletedBehaviour = completedBehaviour;
            bool nextWasTransition = wasTransition;
            int immediateSelectionCount = 0;

            do
            {
                _hasPendingSelection = false;
                SelectNextBehaviour(nextCompletedBehaviour, nextWasTransition);
                immediateSelectionCount++;

                if (currentBehaviour != null || _isWaitingForCandidate || !_hasPendingSelection)
                    return;

                if (immediateSelectionCount >= _maxImmediateSelectionsPerRequest)
                {
                    Debug.LogWarning($"[{name}] Too many immediate behaviour completions. Retrying selection shortly.", this);
                    _hasPendingSelection = false;
                    StartWaitingForCandidate();
                    return;
                }

                nextCompletedBehaviour = _pendingCompletedBehaviour;
                nextWasTransition = _pendingWasTransition;
            } while (true);
        }
        finally
        {
            _isProcessingSelection = false;
        }
    }

    private void SelectNextBehaviour(IEnemyBehaviour completedBehaviour, bool wasTransition)
    {
        if (isDead)
            return;

        RefreshContextForDecision();
        if (enemyBehaviourQueue.Count > 0)
        {
            ExecuteBehaviour(enemyBehaviourQueue.Dequeue());
            return;
        }

        List<WeightedBehaviourCandidate> candidates = GetWeightedCandidates();
        if (candidates.Count == 0)
        {
            StartWaitingForCandidate();
            return;
        }

        RemoveCompletedBehaviourWhenAlternativesExist(candidates, completedBehaviour, wasTransition);
        ExecuteBehaviour(PickWeightedBehaviour(candidates));
    }

    private void RefreshContextForDecision()
    {
        if (_context != null && _context.Target == null)
        {
            if (_contextTarget != null)
                _context.SetTarget(_contextTarget);
            else if (PlayerStateMachine.instance != null)
                _context.SetTarget(PlayerStateMachine.instance.transform);
        }

        _context?.RefreshForDecision(transform, currentPhase, currentBehaviour, _exchangeMemoryDuration);
    }

    private List<WeightedBehaviourCandidate> GetWeightedCandidates()
    {
        List<WeightedBehaviourCandidate> candidates = new List<WeightedBehaviourCandidate>();

        if (enemyBehaviours == null)
            return candidates;

        foreach (IEnemyBehaviour behaviour in enemyBehaviours)
        {
            if (behaviour == null || !IsBehaviourEligible(behaviour, out float weight))
                continue;

            candidates.Add(new WeightedBehaviourCandidate(behaviour, weight));
        }

        return candidates;
    }

    private bool IsBehaviourEligible(IEnemyBehaviour behaviour, out float weight)
    {
        weight = 0.0f;

        if (behaviour is IConditionalEnemyBehaviour conditionalBehaviour
            && !conditionalBehaviour.CanExecute(this))
            return false;

        if (behaviour is IContextualEnemyBehaviour contextualBehaviour)
        {
            if (_context == null
                || !contextualBehaviour.CanExecute(_context))
                return false;

            weight = contextualBehaviour.GetWeight(_context);
        }
        else
        {
            weight = 100.0f;
        }

        return weight > 0.0f && !float.IsNaN(weight) && !float.IsInfinity(weight);
    }

    private static void RemoveCompletedBehaviourWhenAlternativesExist(
        List<WeightedBehaviourCandidate> candidates,
        IEnemyBehaviour completedBehaviour,
        bool wasTransition)
    {
        if (wasTransition || completedBehaviour == null || candidates.Count <= 1)
            return;

        int alternativesCount = 0;
        foreach (WeightedBehaviourCandidate candidate in candidates)
        {
            if (candidate.Behaviour != completedBehaviour)
                alternativesCount++;
        }

        if (alternativesCount > 0)
            candidates.RemoveAll(candidate => candidate.Behaviour == completedBehaviour);
    }

    private static IEnemyBehaviour PickWeightedBehaviour(List<WeightedBehaviourCandidate> candidates)
    {
        double totalWeight = 0.0;
        foreach (WeightedBehaviourCandidate candidate in candidates)
            totalWeight += candidate.Weight;

        double roll = UnityEngine.Random.value * totalWeight;
        double cumulativeWeight = 0.0;
        foreach (WeightedBehaviourCandidate candidate in candidates)
        {
            cumulativeWeight += candidate.Weight;
            if (roll < cumulativeWeight)
                return candidate.Behaviour;
        }

        return candidates[candidates.Count - 1].Behaviour;
    }

    private void StartWaitingForCandidate()
    {
        if (!_isWaitingForCandidate)
            Debug.LogWarning($"[{name}] No eligible positive-weight behaviour is configured for the current phase. Retrying shortly.", this);

        _isWaitingForCandidate = true;
        _nextEmptySelectionRetryUnscaledTime = Time.unscaledTime + Mathf.Max(0.05f, _emptySelectionRetryInterval);
    }

    private void TryRetrySelectionAfterWait()
    {
        if (!_isWaitingForCandidate
            || isDead
            || debugMode
            || Time.unscaledTime < _nextEmptySelectionRetryUnscaledTime)
            return;

        _isWaitingForCandidate = false;
        RequestNextBehaviourSelection(null, false);
    }

    private readonly struct WeightedBehaviourCandidate
    {
        public IEnemyBehaviour Behaviour { get; }
        public float Weight { get; }

        public WeightedBehaviourCandidate(IEnemyBehaviour behaviour, float weight)
        {
            Behaviour = behaviour;
            Weight = weight;
        }
    }

    private int GetPhaseCount()
    {
        return phases != null ? phases.Count : 0;
    }

    private List<IEnemyBehaviour> GetPhaseBehaviours(int phaseIndex)
    {
        if (!IsPhaseInList(phaseIndex))
        {
            Debug.LogError($"[{name}] Cannot get behaviours for phase {phaseIndex}: no such phase is configured.", this);
            return new List<IEnemyBehaviour>();
        }

        OdinEnemyPhase phase = phases[phaseIndex];
        if (phase == null)
        {
            Debug.LogError($"[{name}] Phase {phaseIndex} is missing.", this);
            return new List<IEnemyBehaviour>();
        }

        return phase.GetBehaviours();
    }

    private IEnemyBehaviour GetPhaseTransitionBehaviour(int phaseIndex)
    {
        if (!IsPhaseInList(phaseIndex))
        {
            Debug.LogError($"[{name}] Cannot get transition for phase {phaseIndex}: no such phase is configured.", this);
            return null;
        }

        OdinEnemyPhase phase = phases[phaseIndex];
        if (phase == null)
        {
            Debug.LogError($"[{name}] Phase {phaseIndex} is missing.", this);
            return null;
        }

        return phase.transitionBehaviour;
    }

    private bool IsPhaseInList(int phaseIndex)
    {
        return phaseIndex >= 0 && phaseIndex < GetPhaseCount();
    }

    private IEnumerable<Type> GetInlineBehaviourTypes()
    {
        return EnemyBehaviourTypeUtility.GetBehaviourTypes(this);
    }

    private void OnEnable()
    {
        BindPhaseOwners();
    }

    private void OnDisable()
    {
        ClearRoot();
    }

    private void OnValidate()
    {
        BindPhaseOwners();
    }

    private void BindPhaseOwners()
    {
        if (phases == null)
            return;

        foreach (OdinEnemyPhase phase in phases)
        {
            if (phase != null)
                phase.BindOwner(this);
        }
    }

    public void DeactivateHitbox()
    {
        if (EnemyHolder.instance != null)
            EnemyHolder.instance.UnRegisterEnemy(gameObject);

        if (sphereCollider != null)
            sphereCollider.enabled = false;
    }

    public void ActivateHitbox()
    {
        EnemyHolder.instance.RegisterEnemy(gameObject, true);
        sphereCollider.enabled = true;

        if (!hasSpawned)
        {
            hasSpawned = true;
            OnSpawnBoss?.Invoke();
        }
    }

    [ShowIf(nameof(debugMode))]
    [Button("Execute Debug Behaviour")]
    private void ExecuteDebugBehaviour()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning($"[{name}] Cannot execute debug behaviour in edit mode.", this);
            return;
        }

        if (debugBehaviour == null)
        {
            Debug.LogWarning($"[{name}] No debug behaviour is configured.", this);
            return;
        }

        RequestDebugBehaviourExecution();
    }

    private void RequestDebugBehaviourExecution()
    {
        if (_isProcessingDebugExecution)
        {
            _hasPendingDebugExecution = true;
            return;
        }

        _isProcessingDebugExecution = true;
        try
        {
            int immediateExecutionCount = 0;
            do
            {
                _hasPendingDebugExecution = false;
                ExecuteDebugBehaviourNow();
                immediateExecutionCount++;

                if (currentBehaviour != null || !_hasPendingDebugExecution)
                    return;

                if (immediateExecutionCount >= _maxImmediateSelectionsPerRequest)
                {
                    Debug.LogWarning($"[{name}] Too many immediate debug behaviour completions. Debug execution stopped.", this);
                    _hasPendingDebugExecution = false;
                    return;
                }
            } while (true);
        }
        finally
        {
            _isProcessingDebugExecution = false;
        }
    }

    private void ExecuteDebugBehaviourNow()
    {
        InterruptCurrentBehaviour();

        if (enemyBehaviourQueue.Count > 0)
            ExecuteBehaviour(enemyBehaviourQueue.Dequeue());
        else
            ExecuteBehaviour(debugBehaviour);
    }

    [ShowIf(nameof(debugMode))]
    [Button("Stop Debug Behaviour")]
    private void StopDebugBehaviour()
    {
        InterruptCurrentBehaviour();
    }
}
