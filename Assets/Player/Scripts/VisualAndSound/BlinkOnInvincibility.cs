using System;
using System.Collections;
using Game_Manager;
using Player.Scripts;
using Tools_and_Scripts;
using UnityEngine;

public class BlinkOnInvincibility : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerGraphic;

    private PlayerStateMachine player;
    private SqueezeAndStretch squeezeAndStretch;

    private void Start()
    {
        squeezeAndStretch = GetComponent<SqueezeAndStretch>();

        player = PlayerStateMachine.instance;
        player.playerStagger.OnStagger.AddListener(() =>
        {
            if (!player.playerHealth.IsDead)
            {
                StopAllCoroutines();
                StartCoroutine(BlinkCoroutine());
            }
        });
        GameManager.OnBeforeRestartLevel.AddListener(() =>
        {
            StopAllCoroutines();
            Tools.SetSpriteAlpha(playerGraphic, 1.0f);
        });

    }

    private IEnumerator BlinkCoroutine()
    {
        yield return new WaitWhile(() => player.currentBehaviour.GetBehaviourType() == BehaviourType.Stagger);

        while (player.playerHealth.IsInvincibleFromLastHit)
        {
            yield return new WaitForSeconds(0.2f);
            Tools.SetSpriteAlpha(playerGraphic, 0.1f);
            yield return new WaitForSeconds(0.1f);
            Tools.SetSpriteAlpha(playerGraphic, 1.0f);
        }

        Tools.SetSpriteAlpha(playerGraphic, 1.0f);
        squeezeAndStretch.Trigger();
    }
}
