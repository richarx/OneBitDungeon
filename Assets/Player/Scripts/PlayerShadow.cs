using Player.Scripts;
using UnityEngine;

public class PlayerShadow : MonoBehaviour
{
    private void Start()
    {
        Animator animator = GetComponent<Animator>();

        PlayerStateMachine.instance.playerJump.OnStartJump.AddListener(() => animator.Play("Jump", 0, 0.0f));
    }
}
