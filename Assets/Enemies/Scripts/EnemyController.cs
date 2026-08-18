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
using UnityEngine.Serialization;

public class EnemyController : SerializedMonoBehaviour
{
    [TitleGroup("Mort legacy")]
    [ShowIf(nameof(UsesLegacyPhaseDefinitions))]
    public GameObject deathBehaviourObject;

    // Exposed properties
    [TitleGroup("Phases")]
    [OdinSerialize]
    [OnValueChanged(nameof(BindOdinPhaseOwners))]
    [ListDrawerSettings(ShowFoldout = true)]
    [LabelText("Phases Odin")]
    private List<OdinEnemyPhase> odinEnemyPhases = new List<OdinEnemyPhase>();

    [TitleGroup("Mort Odin")]
    [ShowIf(nameof(UsesOdinPhaseDefinitions))]
    [OdinSerialize]
    [LabelText("Comportement de mort")]
    [HideReferenceObjectPicker]
    [TypeFilter(nameof(GetInlineBehaviourTypes))]
    private IEnemyBehaviour odinDeathBehaviour;

    [TitleGroup("Phases legacy")]
    [ShowIf(nameof(UsesLegacyPhaseDefinitions))]
    public List<EnemyPhase> enemyPhases;

    [field: SerializeField] public SpriteRenderer Sprite { get; private set; }
    [field: SerializeField] public SpriteRenderer shadowSprite { get; private set; }


    // Runtime Components and state
    [field: NonSerialized] public Animator animator { get; private set; }
    [field: NonSerialized] public Damageable damageable { get; private set; }
    [field: NonSerialized] public AfterImage afterImage { get; private set; }
    [NonSerialized] private SphereCollider sphereCollider;

    [NonSerialized] private bool isDead;
    [NonSerialized] private readonly Dictionary<GameObject, IEnemyBehaviour> runtimeBehaviourCache = new Dictionary<GameObject, IEnemyBehaviour>();
    [NonSerialized] private Transform runtimeBehaviourRoot;
    [NonSerialized] private BehaviourExecution activeExecution;
    [NonSerialized] private int executionId;



    // Behaviour and phase state

    [NonSerialized] public UnityEvent OnChangeBehaviour = new UnityEvent();

    [NonSerialized] public List<IEnemyBehaviour> enemyBehaviours;
    [field: NonSerialized] public IEnemyBehaviour currentBehaviour { get; private set; }
    [field: NonSerialized] public IEnemyBehaviour startingBehaviour { get; private set; }
    [field: NonSerialized] public IEnemyBehaviour phaseTransitionBehaviour { get; private set; }

    [field: NonSerialized] public int currentPhase { get; private set; } = 0;
    private bool isLastPhase => currentPhase >= GetPhaseCount() - 1;
    private bool UsesOdinPhaseDefinitions => odinEnemyPhases != null && odinEnemyPhases.Count > 0;
    private bool UsesLegacyPhaseDefinitions => !UsesOdinPhaseDefinitions;




    protected virtual void Start()
    {
        BindOdinPhaseOwners();
        ResetRuntimeState();
        animator = Sprite.GetComponent<Animator>();
        sphereCollider = GetComponent<SphereCollider>();
        damageable = GetComponent<Damageable>();
        afterImage = GetComponent<AfterImage>();

        damageable.OnTakeDamage.AddListener(() =>
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
        runtimeBehaviourCache.Clear();
        runtimeBehaviourRoot = null;
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
        if (UsesOdinPhaseDefinitions)
        {
            if (odinDeathBehaviour == null)
                Debug.LogWarning($"[{name}] No Odin death behaviour is configured; applying the safe death fallback.", this);

            return odinDeathBehaviour;
        }

        IEnemyBehaviour deathBehaviour = ResolveBehaviour(deathBehaviourObject);
        if (deathBehaviour == null)
            Debug.LogError($"[{name}] Could not resolve a valid death behaviour.", this);

        return deathBehaviour;
    }

    public IEnemyBehaviour ResolveBehaviour(GameObject behaviourPrefab)
    {
        if (behaviourPrefab == null)
        {
            Debug.LogError($"[{name}] Cannot resolve a missing behaviour prefab.", this);
            return null;
        }

        if (runtimeBehaviourCache.TryGetValue(behaviourPrefab, out IEnemyBehaviour cachedBehaviour))
        {
            return cachedBehaviour;
        }

        GameObject runtimeBehaviourObject = Instantiate(behaviourPrefab, GetRuntimeBehaviourRoot());
        if (runtimeBehaviourObject == null)
        {
            Debug.LogError($"[{name}] Failed to instantiate behaviour prefab '{behaviourPrefab.name}'.", behaviourPrefab);
            runtimeBehaviourCache.Add(behaviourPrefab, null);
            return null;
        }

        runtimeBehaviourObject.name = $"{behaviourPrefab.name} (Runtime)";
        IEnemyBehaviour runtimeBehaviour = runtimeBehaviourObject.GetComponent<IEnemyBehaviour>();
        if (runtimeBehaviour == null)
        {
            Debug.LogError($"[{name}] Behaviour prefab '{behaviourPrefab.name}' does not implement IEnemyBehaviour.", behaviourPrefab);
            runtimeBehaviourCache.Add(behaviourPrefab, null);
            Destroy(runtimeBehaviourObject);
            return null;
        }

        runtimeBehaviourCache.Add(behaviourPrefab, runtimeBehaviour);
        return runtimeBehaviour;
    }

    private Transform GetRuntimeBehaviourRoot()
    {
        if (runtimeBehaviourRoot != null)
            return runtimeBehaviourRoot;

        runtimeBehaviourRoot = new GameObject("Runtime Behaviours").transform;
        runtimeBehaviourRoot.SetParent(transform, false);
        return runtimeBehaviourRoot;
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
            return;

        IEnemyBehaviour completedBehaviour = currentBehaviour;
        bool wasTransition = completedBehaviour == startingBehaviour;

        RemoveCurrentBehaviourAndExecution();
        completedBehaviour.StopBehaviour(this);

        SelectNextBehaviour(completedBehaviour, wasTransition);
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

        return enemyBehaviours.FindAll(behaviour => behaviour != null);
    }

    private int GetPhaseCount()
    {
        return UsesOdinPhaseDefinitions ? odinEnemyPhases.Count : enemyPhases != null ? enemyPhases.Count : 0;
    }

    private int GetPhaseHealthThreshold(int phaseIndex)
    {
        if (!IsPhaseInList(phaseIndex))
            return 0;

        if (UsesOdinPhaseDefinitions)
        {
            OdinEnemyPhase phase = odinEnemyPhases[phaseIndex];
            return phase != null ? phase.healthThresholdToTriggerTransition : 0;
        }

        EnemyPhase legacyPhase = enemyPhases[phaseIndex];
        return legacyPhase != null ? legacyPhase.healthThresholdToTriggerTransition : 0;
    }

    private List<IEnemyBehaviour> GetPhaseBehaviours(int phaseIndex)
    {
        if (!IsPhaseInList(phaseIndex))
        {
            Debug.LogError($"[{name}] Cannot get behaviours for phase {phaseIndex}: the active phase source has no such phase.", this);
            return new List<IEnemyBehaviour>();
        }

        if (UsesOdinPhaseDefinitions)
        {
            OdinEnemyPhase phase = odinEnemyPhases[phaseIndex];
            if (phase == null)
            {
                Debug.LogError($"[{name}] Odin phase {phaseIndex} is missing.", this);
                return new List<IEnemyBehaviour>();
            }

            return phase.GetBehaviours();
        }

        EnemyPhase legacyPhase = enemyPhases[phaseIndex];
        if (legacyPhase == null)
        {
            Debug.LogError($"[{name}] Legacy phase {phaseIndex} is missing.", this);
            return new List<IEnemyBehaviour>();
        }

        return legacyPhase.GetBehaviours(this);
    }

    private IEnemyBehaviour GetPhaseTransitionBehaviour(int phaseIndex)
    {
        if (!IsPhaseInList(phaseIndex))
        {
            Debug.LogError($"[{name}] Cannot get transition for phase {phaseIndex}: the active phase source has no such phase.", this);
            return null;
        }

        if (UsesOdinPhaseDefinitions)
        {
            OdinEnemyPhase phase = odinEnemyPhases[phaseIndex];
            if (phase == null)
            {
                Debug.LogError($"[{name}] Odin phase {phaseIndex} is missing.", this);
                return null;
            }

            return phase.transitionBehaviour;
        }

        EnemyPhase legacyPhase = enemyPhases[phaseIndex];
        if (legacyPhase == null)
        {
            Debug.LogError($"[{name}] Legacy phase {phaseIndex} is missing.", this);
            return null;
        }

        return legacyPhase.GetTransitionBehaviour(this);
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
        BindOdinPhaseOwners();
    }

    private void OnValidate()
    {
        BindOdinPhaseOwners();
    }

    private void BindOdinPhaseOwners()
    {
        if (odinEnemyPhases == null)
            return;

        foreach (OdinEnemyPhase phase in odinEnemyPhases)
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
}
