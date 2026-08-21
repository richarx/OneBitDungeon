using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoPunchComboData", menuName = "ScriptableObjects/Biscotto/Punch Combo Data")]
public sealed class BiscottoPunchComboData : ScriptableObject
{
    [field: SerializeField]
    [field: LabelText("Nom du pattern")]
    public string PatternName { get; private set; } = "Grosse Patate";

    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone rectangulaire")]
    public GameObject RectangularDamageZonePrefab { get; private set; }

    [field: SerializeField]
    [field: ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
    [field: LabelText("Coups")]
    public List<BiscottoPunchStep> PunchSteps { get; private set; } = new List<BiscottoPunchStep>();

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la visée")]
    public float RotationDampening { get; private set; } = 0.08f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération finale")]
    [field: SuffixLabel("secondes")]
    public float FinalRecoveryDuration { get; private set; } = 0.8f;

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = true;
}
