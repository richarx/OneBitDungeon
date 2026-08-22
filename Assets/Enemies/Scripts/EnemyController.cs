using System;
using System.Collections.Generic;
using Enemies.Scripts;
using Enemies.Scripts.Behaviours;
using Enemies.Spawner;
using Game_Manager;
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

    [TitleGroup("Mort")]
    [OdinSerialize]
    [LabelText("Death Behaviour")]
    [HideReferenceObjectPicker]
    [TypeFilter(nameof(GetInlineBehaviourTypes))]
    private IEnemyBehaviour deathBehaviour;

    [field: SerializeField] public SpriteRenderer Sprite { get; private set; }
    [field: SerializeField] public SpriteRenderer shadowSprite { get; private set; }


    // Runtime Components and state
    public Animator animator { get; private set; }
    public Damageable damageable { get; private set; }
    public AfterImage afterImage { get; private set; }
    private SphereCollider sphereCollider;

    private bool isDead;
    private BehaviourExecution activeExecution;
    private int executionId;



    // Behaviour and phase state

    [NonSerialized] public UnityEvent OnChangeBehaviour = new UnityEvent();

    private List<IEnemyBehaviour> enemyBehaviours;
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


    protected virtual void Start()
    {
        BindPhaseOwners();

        ResetRuntimeState();
        animator = Sprite.GetComponent<Animator>();
        sphereCollider = GetComponent<SphereCollider>();
        damageable = GetComponent<Damageable>();
        afterImage = GetComponent<AfterImage>();

        if (debugMode)
        {
             EnemyHolder.instance.RegisterEnemy(gameObject);
             return;
        }

        damageable.OnTakeDamage.AddListener((_) =>
        {
            if (!isDead && !isLastPhase && currentBehaviour != startingBehaviour && damageable.currentHealth <= GetPhaseHealthThreshold(currentPhase + 1))
            {
                Debug.Log("Trigger Next Phase !");
                currentPhase += 1;
                enemyBehaviours = GetPhaseBehaviours(currentPhase);
                InterruptCurrentBehaviour();
                startingBehaviour = GetPhaseTransitionBehaviour(currentPhase);
                ExecuteBehaviour(startingBehaviour);
            }
        });

        damageable.OnDie.AddListener(() =>
        {
            if (isDead)
                return;

            isDead = true;
            InterruptCurrentBehaviour();

            IEnemyBehaviour deathBehaviour = GetDeathBehaviour();
            if (deathBehaviour != null)
            {
                ExecuteBehaviour(deathBehaviour);
                return;
            }

            HandleMissingDeathBehaviour();
        });

        if (GetPhaseCount() == 0)
        {
            Debug.LogError($"[{name}] No enemy phase is configured.", this);
            return;
        }

        enemyBehaviours = GetPhaseBehaviours(currentPhase);
        startingBehaviour = GetPhaseTransitionBehaviour(currentPhase);
        ExecuteBehaviour(startingBehaviour);
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

        if (Sprite != null)
            Sprite.enabled = false;

        if (shadowSprite != null)
            shadowSprite.enabled = false;

        Debug.LogWarning($"[{name}] Applied the safe death fallback and unlocked the level.", this);
        GameManager.OnUnlockLevel?.Invoke();
    }

    protected virtual void Update()
    {
        if (currentBehaviour != null)
            currentBehaviour.UpdateBehaviour(this);
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
            SelectNextBehaviour(completedBehaviour, wasTransition);
        else
            ExecuteDebugBehaviour();

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
    }

    private void SelectNextBehaviour(IEnemyBehaviour completedBehaviour, bool wasTransition)
    {
        if (isDead)
            return;

        List<IEnemyBehaviour> validBehaviours = GetValidBehaviours();
        if (validBehaviours.Count == 0)
        {
            Debug.LogWarning($"[{name}] No valid attack behaviour is configured for the current phase.", this);
            return;
        }

        if (!wasTransition && validBehaviours.Count > 1 && validBehaviours.Contains(completedBehaviour))
            validBehaviours.Remove(completedBehaviour);

        ExecuteBehaviour(validBehaviours[UnityEngine.Random.Range(0, validBehaviours.Count)]);
    }

    private List<IEnemyBehaviour> GetValidBehaviours()
    {
        if (enemyBehaviours == null)
            return new List<IEnemyBehaviour>();

        return enemyBehaviours.FindAll(behaviour =>
            behaviour != null
            && (!(behaviour is IConditionalEnemyBehaviour conditionalBehaviour)
                || conditionalBehaviour.CanExecute(this)));
    }

    private int GetPhaseCount()
    {
        return phases != null ? phases.Count : 0;
    }

    private int GetPhaseHealthThreshold(int phaseIndex)
    {
        if (!IsPhaseInList(phaseIndex))
            return 0;

        OdinEnemyPhase phase = phases[phaseIndex];
        return phase != null ? phase.healthThresholdToTriggerTransition : 0;
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
        EnemyHolder.instance.RegisterEnemy(gameObject);
        sphereCollider.enabled = true;
    }

    [ShowIf(nameof(debugMode))]
    [Button("Execute Debug Behaviour")]
    private void ExecuteDebugBehaviour()
    {
        if (debugBehaviour == null)
        {
            Debug.LogWarning($"[{name}] No debug behaviour is configured.", this);
            return;
        }

        InterruptCurrentBehaviour();
        ExecuteBehaviour(debugBehaviour);
    }

    [ShowIf(nameof(debugMode))]
    [Button("Stop Debug Behaviour")]
    private void StopDebugBehaviour()
    {
        InterruptCurrentBehaviour();
    }
}
