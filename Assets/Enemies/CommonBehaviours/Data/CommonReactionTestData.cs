using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CommonReactionTestData", menuName = "ScriptableObjects/Common Behaviours/Context Weight Test Data")]
public sealed class CommonReactionTestData : ScriptableObject
{
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Default weight")]
    public float DefaultWeight { get; private set; } = 100.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Close distance")]
    [field: SuffixLabel("meters")]
    public float CloseDistance { get; private set; } = 3.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Weight while close")]
    public float CloseWeight { get; private set; } = 100.0f;

    [field: SerializeField]
    [field: LabelText("Eligible")]
    public bool IsEligible { get; private set; } = true;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Auto-complete after")]
    [field: SuffixLabel("seconds")]
    public float AutoCompleteAfter { get; private set; } = 1.0f;
}
