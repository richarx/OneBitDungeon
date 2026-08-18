using System;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class CameraZoomer : MonoBehaviour
{
    [SerializeField] private float zoomPowerOnParry;
    [SerializeField] private float zoomDurationOnParry;

    [Space]
    [SerializeField] private float zoomPowerOnDodge;
    [SerializeField] private float zoomDurationOnDodge;

    [Space]
    [SerializeField] private Ease easeIn;
    [SerializeField] private Ease easeOut;

    private Camera mainCamera;
    private Camera decorCamera;

    private float startingFov = 60.0f;

    private Sequence currentSequence;

    private void Start()
    {
        mainCamera = CamerasHolder.instance.mainCamera;
        decorCamera = CamerasHolder.instance.decorCamera;

        PlayerStateMachine player = PlayerStateMachine.instance;

        player.playerParry.OnSuccessfulParry.AddListener(() => StartZoom(zoomDurationOnParry, zoomDurationOnParry, zoomPowerOnParry));
        player.playerCriticalAttack.OnPlayerAttack.AddListener(HandleCritStrike);
        player.playerCriticalAttack.OnReachedTarget.AddListener(HandleCritStrikeReachedTarget);

        ArroganceGainEvents.OnGainProcessed += HandleArrogantDodge;
    }

    private void HandleCritStrike(AttackPayload payload)
    {
        StartZoom(0.7f, 0.3f, 10.0f);
    }

    private void HandleCritStrikeReachedTarget()
    {

    }

    private void HandleArrogantDodge(ArroganceGainResult result)
    {
        if (result.request.reason != ArroganceGainReason.CloseDodge || PlayerStateMachine.instance.playerArrogantSpin.TimeSinceLastSpin >= 0.5f)
            return;

        StartZoom(zoomDurationOnDodge, zoomDurationOnDodge, zoomPowerOnDodge);
    }

    private void StartZoom(float durationIn, float durationOut, float zoomPower)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create(useUnscaledTime: true)
            .Group(Tween.CameraFieldOfView(mainCamera, startingFov - zoomPower, durationIn, easeIn))
            .Group(Tween.CameraFieldOfView(decorCamera, startingFov - zoomPower, durationIn, easeIn))
            .Chain(Tween.CameraFieldOfView(mainCamera, startingFov, durationOut, easeOut))
            .Group(Tween.CameraFieldOfView(decorCamera, startingFov, durationOut, easeOut));
    }
}
