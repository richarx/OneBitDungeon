using UnityEngine;

namespace Player
{
    public class PlayerSword : MonoBehaviour
    {
        [SerializeField] private bool hasSword;

        private PlayerStateMachine player;
        
        private bool currentlyHasSword;
        public bool CurrentlyHasSword => currentlyHasSword;
        private bool isSwordInHand;
        public bool IsSwordInHand => isSwordInHand;
    
        private void Start()
        {
            currentlyHasSword = hasSword;
            player = PlayerStateMachine.instance;
        }

        private void LateUpdate()
        {
            if (hasSword != currentlyHasSword)
            {
                currentlyHasSword = hasSword;
                isSwordInHand = false;
            }

            if (currentlyHasSword && player.inputPackage.northButton.wasPressedThisFrame)
                isSwordInHand = !isSwordInHand;
        }
    }
}
