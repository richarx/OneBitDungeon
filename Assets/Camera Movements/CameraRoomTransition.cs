using System;
using Camera_Movements;
using Decor.Door;
using Game_Manager;
using PrimeTween;
using Tools_and_Scripts;
using UnityEngine;

public class CameraRoomTransition : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private float delay;
    [SerializeField] private Ease ease;
    [SerializeField] private bool isFollowingRotation;

    [Space]
    [SerializeField] private CameraFollowPlayer cameraFollowPlayer;

    [Space]
    [SerializeField] private Transform northTarget;
    [SerializeField] private Transform eastTarget;
    [SerializeField] private Transform southTarget;
    [SerializeField] private Transform westTarget;

    private void Start()
    {
        GameManager.OnPrepareToLeaveRoom.AddListener(TriggerTransition);
    }

    private void TriggerTransition(DoorController.DoorSide doorSide)
    {
        cameraFollowPlayer.SetLockState(true);

        Transform target = ComputeTarget(doorSide);

        if (isFollowingRotation)
        {
            Sequence.Create()
                .Group(Tween.Position(transform, target.position, duration, ease, startDelay: delay))
                .Group(Tween.Rotation(transform, target.rotation, duration, ease, startDelay: delay));
        }
        else
        {
            Sequence.Create()
                .Group(Tween.Position(transform, target.position, duration, ease, startDelay: delay));
        }

    }

    private Transform ComputeTarget(DoorController.DoorSide doorSide)
    {
        switch (doorSide.Opposite())
        {
            default:
            case DoorController.DoorSide.North:
                return northTarget;
            case DoorController.DoorSide.East:
                return eastTarget;
            case DoorController.DoorSide.South:
                return southTarget;
            case DoorController.DoorSide.West:
                return westTarget;
        }
    }
}
