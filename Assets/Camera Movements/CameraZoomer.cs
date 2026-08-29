using System;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class CameraZoomer : MonoBehaviour
{
    private Camera mainCamera;
    private Camera decorCamera;

    private float startingFov = 60.0f;

    private Sequence currentSequence;

    private void Start()
    {
        PlayerStateMachine player = PlayerStateMachine.instance;

        player.playerParry.OnSuccessfulParry.AddListener(() => StartZoom(0.1f, 0.3f, 8.0f));
        player.playerParry.OnSuccessfulBlock.AddListener(() => StartZoom(0.1f, 0.3f, 1.0f));
        player.playerAttack.OnPlayerAttack.AddListener(HandleAttack);
        player.playerHealth.OnPlayerTakeDamage.AddListener((_) => StartZoom(0.1f, 0.5f, 12.0f));

        ArroganceGainEvents.OnGainProcessed += HandleArrogantDodge;
    }

    private void SetupCameras()
    {
        mainCamera = CamerasHolder.instance.mainCamera;
        decorCamera = CamerasHolder.instance.decorCamera;
    }

    private void HandleAttack(AttackPayload payload)
    {
        if (payload.Type == AttackType.Critical)
            StartZoom(0.7f, 0.3f, 10.0f);
    }

    private void HandleArrogantDodge(ArroganceGainResult result)
    {
        if (result.request.reason != ArroganceGainReason.CloseDodge || PlayerStateMachine.instance.playerArrogantSpin.TimeSinceLastSpin >= 0.5f)
            return;

        StartZoom(0.5f, 0.5f, 8.0f);
    }

    private void StartZoom(float durationIn, float durationOut, float zoomPower)
    {
        if (mainCamera == null || decorCamera == null)
            SetupCameras();

        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create(useUnscaledTime: true)
            .Group(Tween.CameraFieldOfView(mainCamera, startingFov - zoomPower, durationIn, Ease.OutCirc))
            .Group(Tween.CameraFieldOfView(decorCamera, startingFov - zoomPower, durationIn, Ease.OutCirc))
            .Chain(Tween.CameraFieldOfView(mainCamera, startingFov, durationOut, Ease.OutBack))
            .Group(Tween.CameraFieldOfView(decorCamera, startingFov, durationOut, Ease.OutBack));
    }
}
