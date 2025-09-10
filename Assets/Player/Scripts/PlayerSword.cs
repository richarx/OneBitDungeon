using UnityEngine;
using UnityEngine.Events;

namespace Player.Scripts
{
    public class PlayerSword : MonoBehaviour
    {
        [SerializeField] private bool hasSword;

        [HideInInspector] public UnityEvent OnEquipSword = new UnityEvent();
        [HideInInspector] public UnityEvent OnSheatheSword = new UnityEvent();

        private PlayerStateMachine player;
        
        private bool currentlyHasSword;
        public bool CurrentlyHasSword => currentlyHasSword;
        private bool isSwordInHand;
        public bool IsSwordInHand => isSwordInHand;
    
        private void Start()
        {
            currentlyHasSword = hasSword;
            player = PlayerStateMachine.instance;
            player.playerAttack.OnPlayerAttack.AddListener((_) => isSwordInHand = true);
        }

        private void LateUpdate()
        {
            if (hasSword != currentlyHasSword)
            {
                currentlyHasSword = hasSword;
                isSwordInHand = false;
            }

            if (currentlyHasSword && player.inputPackage.northButton.wasPressedThisFrame)
            {
                isSwordInHand = !isSwordInHand;
                
                if (isSwordInHand)
                    OnEquipSword?.Invoke();
                else
                    OnSheatheSword?.Invoke();
            }
        }
    }
}
