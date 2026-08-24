using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoFlexData", menuName = "ScriptableObjects/Biscotto/Flex Data")]
public sealed class BiscottoFlexData : ScriptableObject
{
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du flex")]
    public float FlexDuration { get; private set; } = 1.2f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération")]
    public float RecoveryDuration { get; private set; } = 0.35f;

    [field: SerializeField]
    [field: LabelText("Animation Arrogance 1")]
    public string FlexAnimation_Arrogance_1 { get; private set; }
    [field: SerializeField]
    [field: LabelText("Animation Arrogance 2")]
    public string FlexAnimation_Arrogance_2 { get; private set; }
    [field: SerializeField]
    [field: LabelText("Animation Arrogance 3")]
    public string FlexAnimation_Arrogance_3 { get; private set; }
}
