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

        PlayerStateMachine.instance.playerParry.OnSuccessfulParry.AddListener(() => StartZoom(zoomDurationOnParry, zoomPowerOnParry));
        ArroganceGainEvents.OnGainProcessed += HandleArrogantDodge;
    }

    private void HandleArrogantDodge(ArroganceGainResult result)
    {
        if (result.request.reason != ArroganceGainReason.CloseDodge || PlayerStateMachine.instance.playerArrogantSpin.TimeSinceLastSpin >= 0.5f)
            return;

        StartZoom(zoomDurationOnDodge, zoomPowerOnDodge);
    }

    private void StartZoom(float duration, float zoomPower)
    {
        if (currentSequence.isAlive)
            currentSequence.Stop();

        currentSequence = Sequence.Create(useUnscaledTime: true)
            .Group(Tween.CameraFieldOfView(mainCamera, startingFov - zoomPower, duration, easeIn))
            .Group(Tween.CameraFieldOfView(decorCamera, startingFov - zoomPower, duration, easeIn))
            .Chain(Tween.CameraFieldOfView(mainCamera, startingFov, duration, easeOut))
            .Group(Tween.CameraFieldOfView(decorCamera, startingFov, duration, easeOut));
    }
}
