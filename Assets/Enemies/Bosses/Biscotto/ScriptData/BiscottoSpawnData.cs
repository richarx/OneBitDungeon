using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoSpawnData", menuName = "ScriptableObjects/Biscotto/Spawn Data")]
public sealed class BiscottoSpawnData : ScriptableObject
{
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Délai d'apparition")]
    public float SpawnDelay { get; private set; } = 3.0f;
}
