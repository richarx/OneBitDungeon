using Player.Scripts;
using UnityEngine;

public class PlayerStaminaVfx : MonoBehaviour
{
    [SerializeField] private Animator outOfBreathVfx;
    [SerializeField] private Transform outOfBreathPivot;

    private bool isVfxOnTheRight => outOfBreathPivot.localScale.x > 0;

    private PlayerStateMachine player;
    private PlayerStamina playerStamina;

    private void Start()
    {
        player = GetComponent<PlayerStateMachine>();
        playerStamina = GetComponent<PlayerStamina>();
    }

    private void LateUpdate()
    {
        bool isEmpty = playerStamina.IsEmpty;
        outOfBreathVfx.SetBool("IsBreathing", isEmpty);

        if (isEmpty)
        {
            bool isPlayerLookingRight = player.LastLookDirection.x > 0;

            if (isPlayerLookingRight != isVfxOnTheRight)
                outOfBreathPivot.localScale = new Vector3(isPlayerLookingRight ? 1.0f : -1.0f, 1.0f, 1.0f);
        }
    }
}
