using System;
using Enemies.Scripts.Behaviours;
using Game_Manager;
using PrimeTween;
using UnityEngine;

[Serializable]
public sealed class MageDeathBehaviour : IEnemyBehaviour
{
    [NonSerialized] private Sequence deathSequence;
    [NonSerialized] private bool hasUnlockedLevel;

    public void StartBehaviour(EnemyController enemy, BehaviourExecution execution)
    {
        if (deathSequence.isAlive)
            deathSequence.Stop();

        deathSequence = default;
        hasUnlockedLevel = false;
        Transform camera = CamerasHolder.instance.transform;
        Vector3 startPosition = camera.position;
        Quaternion startRotation = camera.rotation;
        CamerasHolder.instance.cameraFollowPlayer.SetLockState(true);
        enemy.animator.Play("Death");
        enemy.afterImage.Cancel();

        deathSequence = Sequence.Create(useUnscaledTime: true)
            .Group(Tween.GlobalTimeScale(0.15f, 0.5f, Ease.OutCirc))
            .Group(Tween.CameraFieldOfView(CamerasHolder.instance.mainCamera, 45.0f, 0.5f, Ease.InOutBack))
            .Group(Tween.CameraFieldOfView(CamerasHolder.instance.decorCamera, 45.0f, 0.5f, Ease.InOutBack))
            .Group(Tween.PositionY(camera, 9.0f, 0.5f, Ease.OutBack))
            .Group(Tween.PositionX(camera, enemy.transform.position.x, 0.5f, Ease.OutCirc))
            .Group(Tween.PositionZ(camera, enemy.transform.position.z - 6.0f, 0.5f, Ease.OutCirc))
            .Group(Tween.Rotation(camera, new Vector3(40.0f, 0.0f, 0.0f), 0.5f, Ease.OutCirc))
            .ChainDelay(3.5f)
            .Chain(Tween.GlobalTimeScale(1.0f, 0.3f, Ease.InOutExpo))
            .Group(Tween.CameraFieldOfView(CamerasHolder.instance.mainCamera, 60.0f, 0.3f, Ease.InOutBack))
            .Group(Tween.CameraFieldOfView(CamerasHolder.instance.decorCamera, 60.0f, 0.3f, Ease.InOutBack))
            .Group(Tween.Position(camera, startPosition, 0.3f, Ease.OutBack))
            .Group(Tween.Rotation(camera, startRotation, 0.3f, Ease.OutBack))
            .Group(Tween.Alpha(enemy.shadowSprite, 0.0f, 0.3f))
            .ChainCallback(() => CamerasHolder.instance.cameraFollowPlayer.SetLockState(false))
            .Chain(Tween.Rotation(enemy.Sprite.transform, new Vector3(90.0f, 0.0f, 0.0f), 0.5f, Ease.OutBounce))
            .ChainCallback(() => enemy.Sprite.sortingOrder = -1)
            .ChainCallback(() => enemy.DeactivateHitbox())
            .ChainCallback(UnlockLevelOnce);
    }

    public void UpdateBehaviour(EnemyController enemy) { }
    public void FixedUpdateBehaviour(EnemyController enemy) { }
    public void StopBehaviour(EnemyController enemy) { }
    public void CancelBehaviour(EnemyController enemy) { }
    public void SetSubBehaviourState(bool state) { }

    private void UnlockLevelOnce()
    {
        if (hasUnlockedLevel)
            return;

        hasUnlockedLevel = true;
        GameManager.OnUnlockLevel?.Invoke();
    }
}
