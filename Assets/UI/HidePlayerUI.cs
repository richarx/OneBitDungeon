using System;
using Player.Scripts;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

public class HidePlayerUI : MonoBehaviour
{
    private CanvasGroup playerUICanvasGroup;

    private UnityAction<AttackPayload> playerAttackEventHandler;

    private Sequence currentSequence;

    private bool isSetup;

    private void Start()
    {
        playerUICanvasGroup = GetComponent<CanvasGroup>();

        if (!isSetup)
            RegisterListeners();
    }

    private void OnEnable()
    {
        if (PlayerStateMachine.instance != null)
            RegisterListeners();
    }

    private void RegisterListeners()
    {
        PlayerStateMachine player = PlayerStateMachine.instance;

        playerAttackEventHandler = (_) => HideUI();
        player.playerCriticalAttack.OnPlayerAttack.AddListener(playerAttackEventHandler);
        player.playerCriticalAttack.OnReachedTarget.AddListener(DisplayUI);

        isSetup = true;
    }

    private void OnDisable()
    {
        PlayerStateMachine player = PlayerStateMachine.instance;

        player.playerCriticalAttack.OnPlayerAttack.RemoveListener(playerAttackEventHandler);
        player.playerCriticalAttack.OnReachedTarget.RemoveListener(DisplayUI);

        isSetup = false;
    }

    private void HideUI()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.Alpha(playerUICanvasGroup, 0.0f, 0.1f));
    }

    private void DisplayUI()
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create()
            .Chain(Tween.Alpha(playerUICanvasGroup, 1.0f, 0.3f));
    }

    private void HideUIInstant()
    {
        playerUICanvasGroup.alpha = 0.0f;
    }

    private void DisplayUIInstant()
    {
        playerUICanvasGroup.alpha = 1.0f;
    }
}
