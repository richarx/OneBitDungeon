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
    [field: LabelText("Animation")]
    public string FlexAnimation { get; private set; }
}
