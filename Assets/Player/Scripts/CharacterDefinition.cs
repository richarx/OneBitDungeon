
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Player.Scripts
{
    [CreateAssetMenu(fileName = "CharacterDefinition", menuName = "ScriptableObjects/CharacterDefinition")]
    public class CharacterDefinition : SerializedScriptableObject  // ← était ScriptableObject
    {
        [Header("Data")]
        public PlayerData playerData;
        public int maxHealth;

        [Header("Visuals")]
        public AnimationsHolderData animationsHolder;

        [Header("Jump Tag")]
        [OdinSerialize] public IJumpTagStrategy jumpTagStrategy;

        [Header("Attack")]
        [OdinSerialize] public IAttackStrategy attackStrategy;  // ← était [SerializeReference]
    }
}
