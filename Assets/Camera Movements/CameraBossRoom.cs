using System;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class CameraBossRoom : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private Ease ease;

    private Sequence panSequence;

    private void Start()
    {
        transform.position = new Vector3(0.0f, 3.2f, -14.21f);
        transform.rotation = Quaternion.Euler(new Vector3(-15.0f, 0.0f, 0.0f));

        ResetCamera(0.8f);

        EnemyHumility.OnFullHumility.AddListener(PanCamera);
        PlayerStateMachine.instance.playerCriticalAttack.OnStartDash.AddListener(() => ResetCamera());
    }

    private void ResetCamera(float delay = 0.0f)
    {
        if (panSequence.isAlive)
            panSequence.Stop();

        panSequence = Sequence.Create()
            .Group(Tween.Position(transform, new Vector3(0.0f, 15.0f, -17.0f), duration, ease, startDelay: delay))
            .Group(Tween.Rotation(transform, Quaternion.Euler(new Vector3(45.0f, 0.0f, 0.0f)), duration, ease, startDelay: delay));
    }

    private void PanCamera()
    {
        if (panSequence.isAlive)
            panSequence.Stop();

        panSequence = Sequence.Create()
            .Group(Tween.Position(transform, new Vector3(0.0f, 5.0f, -17.0f), duration, ease))
            .Group(Tween.Rotation(transform, Quaternion.Euler(new Vector3(15.0f, 0.0f, 0.0f)), duration, ease));
    }
}
