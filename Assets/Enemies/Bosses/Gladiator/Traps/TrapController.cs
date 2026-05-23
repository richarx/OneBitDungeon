using Enemies.Scripts;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private SpriteRenderer shadowSprite;

    private Animator animator;
    private DealDamageToPlayer dealDamageToPlayer;

    private bool isSetup;
    private bool isLanded;

    public void Setup(Vector3 targetPosition, float trapMoveDuration, float startingHeight)
    {
        animator = spriteTransform.GetComponent<Animator>();
        dealDamageToPlayer = GetComponent<DealDamageToPlayer>();
        GetComponent<Damageable>().OnDie.AddListener(TriggerTrap);
        isSetup = true;

        spriteTransform.localPosition = Vector3.up * startingHeight;

        Sequence.Create()
            .Chain(Tween.Position(transform, targetPosition, trapMoveDuration, Ease.OutCirc))
            .Group(Tween.LocalPositionY(spriteTransform, 0.0f, trapMoveDuration, Ease.InCirc))
            .ChainCallback(() => animator.Play("TrapOpen"))
            .ChainCallback(() => isLanded = true);
    }

    private void Update()
    {
        if (isSetup && isLanded)
            CheckForPlayerDamage();
    }

    private void CheckForPlayerDamage()
    {
        Vector3 directionToPlayer = PlayerStateMachine.instance.position - transform.position;

        bool hitX = Mathf.Abs(directionToPlayer.x) <= 1.5f;
        bool hitZ = Mathf.Abs(directionToPlayer.z) <= 0.3f;
        bool damageApplied = false;

        if (hitX && hitZ)
            damageApplied = dealDamageToPlayer.TryDealDamage(directionToPlayer.normalized);

        if (damageApplied)
            TriggerTrap();
    }

    private void TriggerTrap()
    {
        isSetup = false;
        animator.Play("TrapTrigger");

        Sequence.Create()
            .Chain(Tween.LocalPositionY(spriteTransform, 2.0f, 0.3f, Ease.OutCirc))
            .Chain(Tween.LocalPositionY(spriteTransform, 0.0f, 0.3f, Ease.InCirc))
            .Group(Tween.Alpha(spriteTransform.GetComponent<SpriteRenderer>(), 0.0f, 0.3f))
            .Group(Tween.Alpha(shadowSprite, 0.0f, 0.3f))
            .ChainCallback(() => Destroy(gameObject));
    }
}
