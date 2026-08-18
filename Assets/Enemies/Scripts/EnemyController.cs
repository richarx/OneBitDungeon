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
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

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

    [field: SerializeField, FormerlySerializedAs("sprite")] public SpriteRenderer Sprite { get; private set; }
    [field: SerializeField, FormerlySerializedAs("shadowSprite")] public SpriteRenderer shadowSprite { get; private set; }


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

    [Button("Migrer le Dummy legacy vers Odin", ButtonSizes.Large)]
    [ShowIf(nameof(CanMigrateDummyLegacyToOdin))]
    private void MigrateDummyLegacyToOdin()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.LogError($"[{name}] Stop Play Mode before migrating legacy Dummy data to Odin.", this);
            return;
        }

        if (odinEnemyPhases != null && odinEnemyPhases.Count > 0)
        {
            Debug.LogError($"[{name}] Dummy Odin migration was not applied because an Odin configuration already exists.", this);
            return;
        }

        if (!TryCreateDummyOdinPhase(out OdinEnemyPhase migratedPhase, out string error))
        {
            Debug.LogError($"[{name}] Dummy Odin migration was not applied: {error}", this);
            return;
        }

        Undo.RegisterCompleteObjectUndo(this, "Migrate Dummy Legacy Phase to Odin");
        odinEnemyPhases = new List<OdinEnemyPhase> { migratedPhase };
        BindOdinPhaseOwners();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#else
        Debug.LogError("Dummy Odin migration is only available in the Unity Editor.", this);
#endif
    }

    private bool CanMigrateDummyLegacyToOdin()
    {
        if (odinEnemyPhases != null && odinEnemyPhases.Count > 0)
            return false;

        if (enemyPhases == null || enemyPhases.Count != 1 || enemyPhases[0] == null)
            return false;

        EnemyPhase legacyPhase = enemyPhases[0];
        if (legacyPhase.transitionBehaviour == null || legacyPhase.transitionBehaviour.GetComponent<DummyTransition>() == null)
            return false;

        return legacyPhase.phaseBehaviours != null
               && legacyPhase.phaseBehaviours.Count == 1
               && legacyPhase.phaseBehaviours[0] != null
               && legacyPhase.phaseBehaviours[0].GetComponent<DummyCircleAttack>() != null;
    }

    private bool TryCreateDummyOdinPhase(out OdinEnemyPhase migratedPhase, out string error)
    {
        migratedPhase = null;
        error = null;

        if (enemyPhases == null || enemyPhases.Count != 1 || enemyPhases[0] == null)
        {
            error = "exactly one configured legacy phase is required";
            return false;
        }

        EnemyPhase legacyPhase = enemyPhases[0];
        if (legacyPhase.transitionBehaviour == null || legacyPhase.transitionBehaviour.GetComponent<DummyTransition>() == null)
        {
            error = "the legacy transition must be a DummyTransition";
            return false;
        }

        if (legacyPhase.phaseBehaviours == null || legacyPhase.phaseBehaviours.Count != 1 || legacyPhase.phaseBehaviours[0] == null)
        {
            error = "exactly one configured legacy DummyCircleAttack is required";
            return false;
        }

        DummyCircleAttack legacyAttack = legacyPhase.phaseBehaviours[0].GetComponent<DummyCircleAttack>();
        if (legacyAttack == null || !legacyAttack.TryCreateInlineBehaviour(out DummyCircleAttackBehaviour inlineAttack, out error))
            return false;

        migratedPhase = new OdinEnemyPhase
        {
            healthThresholdToTriggerTransition = legacyPhase.healthThresholdToTriggerTransition,
            transitionBehaviour = new DummyImmediateTransitionBehaviour(),
            phaseBehaviours = new List<IEnemyBehaviour> { inlineAttack }
        };
        return true;
    }

    [Button("Migrer le Gladiateur legacy vers Odin", ButtonSizes.Large)]
    [ShowIf(nameof(CanMigrateGladiatorLegacyToOdin))]
    private void MigrateGladiatorLegacyToOdin()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.LogError($"[{name}] Stop Play Mode before migrating legacy Gladiator data to Odin.", this);
            return;
        }

        if (HasOdinConfiguration())
        {
            Debug.LogError($"[{name}] Gladiator Odin migration was not applied because an Odin configuration already exists.", this);
            return;
        }

        if (!TryCreateGladiatorOdinConfiguration(out List<OdinEnemyPhase> migratedPhases, out IEnemyBehaviour migratedDeathBehaviour, out string error))
        {
            Debug.LogError($"[{name}] Gladiator Odin migration was not applied: {error}", this);
            return;
        }

        Undo.RegisterCompleteObjectUndo(this, "Migrate Gladiator Legacy Behaviours to Odin");
        odinEnemyPhases = migratedPhases;
        odinDeathBehaviour = migratedDeathBehaviour;
        BindOdinPhaseOwners();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#else
        Debug.LogError("Gladiator Odin migration is only available in the Unity Editor.", this);
#endif
    }

    private bool CanMigrateGladiatorLegacyToOdin()
    {
        if (HasOdinConfiguration() || enemyPhases == null || enemyPhases.Count == 0)
            return false;

        if (deathBehaviourObject == null || deathBehaviourObject.GetComponent<GladiatorDeath>() == null)
            return false;

        foreach (EnemyPhase phase in enemyPhases)
        {
            if (phase == null || phase.transitionBehaviour == null || phase.transitionBehaviour.GetComponent<GladiatorSpawn>() == null)
                return false;

            if (phase.phaseBehaviours == null || phase.phaseBehaviours.Count == 0)
                return false;

            foreach (GameObject attack in phase.phaseBehaviours)
            {
                if (!HasExactlyOneGladiatorAttackType(attack))
                    return false;
            }
        }

        return true;
    }

    private bool TryCreateGladiatorOdinConfiguration(out List<OdinEnemyPhase> migratedPhases, out IEnemyBehaviour migratedDeathBehaviour, out string error)
    {
        migratedPhases = null;
        migratedDeathBehaviour = null;
        error = null;

        if (HasOdinConfiguration())
        {
            error = "an Odin configuration already exists";
            return false;
        }

        if (deathBehaviourObject == null)
        {
            error = "the legacy GladiatorDeath behaviour is missing";
            return false;
        }

        GladiatorDeath legacyDeath = deathBehaviourObject.GetComponent<GladiatorDeath>();
        if (legacyDeath == null || !legacyDeath.TryCreateInlineBehaviour(out GladiatorDeathBehaviour inlineDeath, out error))
        {
            if (string.IsNullOrEmpty(error))
                error = "the legacy death behaviour must be a GladiatorDeath";
            return false;
        }

        if (enemyPhases == null || enemyPhases.Count == 0)
        {
            error = "at least one legacy phase is required";
            return false;
        }

        List<OdinEnemyPhase> phases = new List<OdinEnemyPhase>();
        foreach (EnemyPhase legacyPhase in enemyPhases)
        {
            if (!TryCreateGladiatorOdinPhase(legacyPhase, out OdinEnemyPhase migratedPhase, out error))
                return false;

            phases.Add(migratedPhase);
        }

        migratedPhases = phases;
        migratedDeathBehaviour = inlineDeath;
        return true;
    }

    private bool TryCreateGladiatorOdinPhase(EnemyPhase legacyPhase, out OdinEnemyPhase migratedPhase, out string error)
    {
        migratedPhase = null;
        error = null;

        if (legacyPhase == null)
        {
            error = "a legacy phase is missing";
            return false;
        }

        if (legacyPhase.transitionBehaviour == null)
        {
            error = "a GladiatorSpawn transition is missing";
            return false;
        }

        GladiatorSpawn legacySpawn = legacyPhase.transitionBehaviour.GetComponent<GladiatorSpawn>();
        if (legacySpawn == null || !legacySpawn.TryCreateInlineBehaviour(out GladiatorSpawnBehaviour inlineSpawn, out error))
        {
            if (string.IsNullOrEmpty(error))
                error = "the transition must be a GladiatorSpawn";
            return false;
        }

        if (legacyPhase.phaseBehaviours == null || legacyPhase.phaseBehaviours.Count == 0)
        {
            error = "at least one Gladiator attack is required per phase";
            return false;
        }

        List<IEnemyBehaviour> inlineAttacks = new List<IEnemyBehaviour>();
        foreach (GameObject legacyAttack in legacyPhase.phaseBehaviours)
        {
            if (!TryCreateGladiatorInlineAttack(legacyAttack, out IEnemyBehaviour inlineAttack, out error))
                return false;

            inlineAttacks.Add(inlineAttack);
        }

        migratedPhase = new OdinEnemyPhase
        {
            healthThresholdToTriggerTransition = legacyPhase.healthThresholdToTriggerTransition,
            transitionBehaviour = inlineSpawn,
            phaseBehaviours = inlineAttacks
        };
        return true;
    }

    private bool TryCreateGladiatorInlineAttack(GameObject legacyAttack, out IEnemyBehaviour inlineAttack, out string error)
    {
        inlineAttack = null;
        error = null;

        if (!HasExactlyOneGladiatorAttackType(legacyAttack))
        {
            error = "an attack must contain exactly one of GladiatorThrowAxe, GladiatorHook, or GladiatorTraps";
            return false;
        }

        GladiatorThrowAxe legacyAxe = legacyAttack.GetComponent<GladiatorThrowAxe>();
        if (legacyAxe != null)
        {
            if (!legacyAxe.TryCreateInlineBehaviour(out GladiatorThrowAxeBehaviour inlineAxe, out error))
                return false;

            inlineAttack = inlineAxe;
            return true;
        }

        GladiatorHook legacyHook = legacyAttack.GetComponent<GladiatorHook>();
        if (legacyHook != null)
        {
            if (!legacyHook.TryCreateInlineBehaviour(out GladiatorHookBehaviour inlineHook, out error))
                return false;

            inlineAttack = inlineHook;
            return true;
        }

        GladiatorTraps legacyTraps = legacyAttack.GetComponent<GladiatorTraps>();
        if (!legacyTraps.TryCreateInlineBehaviour(out GladiatorTrapsBehaviour inlineTraps, out error))
            return false;

        inlineAttack = inlineTraps;
        return true;
    }

    [Button("Migrer le Mage legacy vers Odin", ButtonSizes.Large)]
    [ShowIf(nameof(CanMigrateMageLegacyToOdin))]
    private void MigrateMageLegacyToOdin()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Debug.LogError($"[{name}] Stop Play Mode before migrating legacy Mage data to Odin.", this);
            return;
        }

        if (!TryCreateMageOdinConfiguration(out List<OdinEnemyPhase> phases, out IEnemyBehaviour death, out string error))
        {
            Debug.LogError($"[{name}] Mage Odin migration was not applied: {error}", this);
            return;
        }

        Undo.RegisterCompleteObjectUndo(this, "Migrate Mage Legacy Behaviours to Odin");
        odinEnemyPhases = phases;
        odinDeathBehaviour = death;
        BindOdinPhaseOwners();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    private bool CanMigrateMageLegacyToOdin()
    {
        if (HasOdinConfiguration() || deathBehaviourObject == null || deathBehaviourObject.GetComponent<MageDeath>() == null)
            return false;

        if (enemyPhases == null || enemyPhases.Count != 3)
            return false;

        Type[] transitions =
        {
            typeof(MageSpawn),
            typeof(MageTransition),
            typeof(MageSecondTransition)
        };
        Type[][] attacks =
        {
            new[]
            {
                typeof(MageEvade),
                typeof(MageSwipeHorizontal),
                typeof(MageSwipeVertical),
                typeof(MageRain),
                typeof(MageThrow)
            },
            new[]
            {
                typeof(MageEvade),
                typeof(MageRain),
                typeof(MageSwipeHorizontal),
                typeof(MageSwipeVertical),
                typeof(MageThrow)
            },
            new[]
            {
                typeof(MageMultiEvade),
                typeof(MageMultiThrow),
                typeof(MageMultiSwipe),
                typeof(MageRain)
            }
        };

        for (int phaseIndex = 0; phaseIndex < enemyPhases.Count; phaseIndex++)
        {
            EnemyPhase phase = enemyPhases[phaseIndex];
            if (phase == null
                || phase.transitionBehaviour == null
                || phase.transitionBehaviour.GetComponent(transitions[phaseIndex]) == null)
            {
                return false;
            }

            if (phase.phaseBehaviours == null || phase.phaseBehaviours.Count != attacks[phaseIndex].Length)
                return false;

            for (int attackIndex = 0; attackIndex < attacks[phaseIndex].Length; attackIndex++)
            {
                GameObject attack = phase.phaseBehaviours[attackIndex];
                if (attack == null || attack.GetComponent(attacks[phaseIndex][attackIndex]) == null)
                    return false;
            }
        }

        return true;
    }

    private bool TryCreateMageOdinConfiguration(out List<OdinEnemyPhase> phases, out IEnemyBehaviour death, out string error)
    {
        phases = null;
        death = null;
        error = null;

        if (HasOdinConfiguration())
        {
            error = "an Odin configuration already exists";
            return false;
        }

        if (deathBehaviourObject == null || deathBehaviourObject.GetComponent<MageDeath>() == null)
        {
            error = "the legacy death behaviour must be MageDeath";
            return false;
        }

        if (!deathBehaviourObject.GetComponent<MageDeath>()
                .TryCreateInlineBehaviour(out MageDeathBehaviour mageDeath, out error))
        {
            return false;
        }

        if (enemyPhases == null || enemyPhases.Count != 3)
        {
            error = "exactly three Mage phases are required";
            return false;
        }

        Type[] transitions =
        {
            typeof(MageSpawn),
            typeof(MageTransition),
            typeof(MageSecondTransition)
        };
        Type[][] attacks =
        {
            new[]
            {
                typeof(MageEvade),
                typeof(MageSwipeHorizontal),
                typeof(MageSwipeVertical),
                typeof(MageRain),
                typeof(MageThrow)
            },
            new[]
            {
                typeof(MageEvade),
                typeof(MageRain),
                typeof(MageSwipeHorizontal),
                typeof(MageSwipeVertical),
                typeof(MageThrow)
            },
            new[]
            {
                typeof(MageMultiEvade),
                typeof(MageMultiThrow),
                typeof(MageMultiSwipe),
                typeof(MageRain)
            }
        };

        List<OdinEnemyPhase> result = new List<OdinEnemyPhase>();

        for (int phaseIndex = 0; phaseIndex < 3; phaseIndex++)
        {
            if (!TryCreateMageOdinPhase(
                    enemyPhases[phaseIndex],
                    transitions[phaseIndex],
                    attacks[phaseIndex],
                    out OdinEnemyPhase phase,
                    out error))
            {
                return false;
            }

            result.Add(phase);
        }

        phases = result;
        death = mageDeath;
        return true;
    }

    private bool TryCreateMageOdinPhase(
        EnemyPhase legacyPhase,
        Type transitionType,
        Type[] attackTypes,
        out OdinEnemyPhase phase,
        out string error)
    {
        phase = null;
        error = null;

        if (legacyPhase == null
            || legacyPhase.transitionBehaviour == null
            || !transitionType.IsInstanceOfType(legacyPhase.transitionBehaviour.GetComponent(transitionType)))
        {
            error = "the Mage transition type does not match the required phase";
            return false;
        }

        if (!TryCreateMageInlineBehaviour(
                legacyPhase.transitionBehaviour,
                out IEnemyBehaviour transition,
                out error))
        {
            return false;
        }

        if (legacyPhase.phaseBehaviours == null
            || legacyPhase.phaseBehaviours.Count != attackTypes.Length)
        {
            error = "the Mage attack count does not match the required phase";
            return false;
        }

        List<IEnemyBehaviour> inlineAttacks = new List<IEnemyBehaviour>();

        for (int attackIndex = 0; attackIndex < attackTypes.Length; attackIndex++)
        {
            GameObject legacyAttack = legacyPhase.phaseBehaviours[attackIndex];

            if (legacyAttack == null
                || !attackTypes[attackIndex].IsInstanceOfType(
                    legacyAttack.GetComponent(attackTypes[attackIndex])))
            {
                error = "a Mage attack type does not match the required phase order";
                return false;
            }

            if (!TryCreateMageInlineBehaviour(
                    legacyAttack,
                    out IEnemyBehaviour inlineAttack,
                    out error))
            {
                return false;
            }

            inlineAttacks.Add(inlineAttack);
        }

        phase = new OdinEnemyPhase
        {
            healthThresholdToTriggerTransition = legacyPhase.healthThresholdToTriggerTransition,
            transitionBehaviour = transition,
            phaseBehaviours = inlineAttacks
        };
        return true;
    }

    private static bool TryCreateMageInlineBehaviour(GameObject legacyObject, out IEnemyBehaviour inlineBehaviour, out string error)
    {
        inlineBehaviour = null;
        error = null;

        if (legacyObject == null)
        {
            error = "a Mage behaviour object is missing";
            return false;
        }

        int count = 0;
        count += legacyObject.GetComponent<MageSpawn>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageTransition>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageSecondTransition>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageDeath>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageEvade>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageMultiEvade>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageSwipeHorizontal>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageSwipeVertical>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageRain>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageThrow>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageMultiThrow>() != null ? 1 : 0;
        count += legacyObject.GetComponent<MageMultiSwipe>() != null ? 1 : 0;

        if (count != 1)
        {
            error = "a Mage behaviour object must contain exactly one supported Mage behaviour";
            return false;
        }

        MageSpawn spawn = legacyObject.GetComponent<MageSpawn>();
        if (spawn != null)
        {
            if (!spawn.TryCreateInlineBehaviour(out MageSpawnBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageTransition transition = legacyObject.GetComponent<MageTransition>();
        if (transition != null)
        {
            if (!transition.TryCreateInlineBehaviour(out MageTransitionBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageSecondTransition second = legacyObject.GetComponent<MageSecondTransition>();
        if (second != null)
        {
            if (!second.TryCreateInlineBehaviour(out MageSecondTransitionBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageEvade evade = legacyObject.GetComponent<MageEvade>();
        if (evade != null)
        {
            if (!evade.TryCreateInlineBehaviour(out MageEvadeBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageMultiEvade multiEvade = legacyObject.GetComponent<MageMultiEvade>();
        if (multiEvade != null)
        {
            if (!multiEvade.TryCreateInlineBehaviour(out MageMultiEvadeBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageSwipeHorizontal horizontal = legacyObject.GetComponent<MageSwipeHorizontal>();
        if (horizontal != null)
        {
            if (!horizontal.TryCreateInlineBehaviour(out MageSwipeHorizontalBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageSwipeVertical vertical = legacyObject.GetComponent<MageSwipeVertical>();
        if (vertical != null)
        {
            if (!vertical.TryCreateInlineBehaviour(out MageSwipeVerticalBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageRain rain = legacyObject.GetComponent<MageRain>();
        if (rain != null)
        {
            if (!rain.TryCreateInlineBehaviour(out MageRainBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageThrow mageThrow = legacyObject.GetComponent<MageThrow>();
        if (mageThrow != null)
        {
            if (!mageThrow.TryCreateInlineBehaviour(out MageThrowBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageMultiThrow multiThrow = legacyObject.GetComponent<MageMultiThrow>();
        if (multiThrow != null)
        {
            if (!multiThrow.TryCreateInlineBehaviour(out MageMultiThrowBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        MageMultiSwipe multiSwipe = legacyObject.GetComponent<MageMultiSwipe>();
        if (multiSwipe != null)
        {
            if (!multiSwipe.TryCreateInlineBehaviour(out MageMultiSwipeBehaviour value, out error))
                return false;

            inlineBehaviour = value;
            return true;
        }

        error = "MageDeath is not valid in a Mage phase";
        return false;
    }

    private bool HasOdinConfiguration()
    {
        return (odinEnemyPhases != null && odinEnemyPhases.Count > 0) || odinDeathBehaviour != null;
    }

    private static bool HasExactlyOneGladiatorAttackType(GameObject attack)
    {
        if (attack == null)
            return false;

        int typeCount = 0;
        typeCount += attack.GetComponent<GladiatorThrowAxe>() != null ? 1 : 0;
        typeCount += attack.GetComponent<GladiatorHook>() != null ? 1 : 0;
        typeCount += attack.GetComponent<GladiatorTraps>() != null ? 1 : 0;
        return typeCount == 1;
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
