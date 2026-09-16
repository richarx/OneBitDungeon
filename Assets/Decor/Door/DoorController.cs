using System;
using Game_Manager;
using Player.Scripts;
using PrimeTween;
using SFX;
using Sirenix.OdinInspector;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Assertions;
using static BossKillSave;

namespace Decor.Door
{
    public class DoorController : MonoBehaviour
    {
        public enum DoorSide
        {
            North,
            East,
            South,
            West
        }

        public enum LockType
        {
            Unlocked,
            LockWhenEnteringRoom,
            UnlockWhenDefeatingBoss,
            BossDoor,
        }

        [SerializeField] private DoorSide doorSide;
        [SerializeField] private bool isSpecialDoor;
        [SerializeField] private string specialTarget;
        [SerializeField] private SceneField targetScene;
        [SerializeField] private LockType lockType;

        private bool isLockedByBoss => lockType == LockType.UnlockWhenDefeatingBoss || lockType == LockType.BossDoor;
        [field: SerializeField]
        [field: ShowIf(nameof(isLockedByBoss))]
        private BossName bossName;

        [Space]
        [SerializeField] private Animator animator;
        [SerializeField] private DoorTrigger trigger;
        [SerializeField] private GameObject hitbox;
        [SerializeField] private SpriteRenderer doorSpriteRenderer;
        [SerializeField] private SpriteRenderer doorwaySpriteRenderer;

        [Space]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        private bool isLocked;
        private bool isCurrentDoor;

        public DoorSide doorDirection => doorSide;
        public bool IsSpecialDoor => isSpecialDoor;
        public string SpecialTarget => specialTarget;

        private void Start()
        {
            Assert.IsNotNull(targetScene, $"In Door : {doorSide} => target scene has not been set");

            trigger.OnTrigger.AddListener(() => GameManager.instance.ChangeSceneFromDoor(targetScene.SceneName, this));

            SetupEvents();
            SetInitialState();
        }

        public void MarkAsCurrentDoor()
        {
            isCurrentDoor = true;
            SetInitialState();
        }

        private void SetupEvents()
        {
            switch (lockType)
            {
                case LockType.Unlocked:
                    break;
                case LockType.LockWhenEnteringRoom:
                    GameManager.OnLockLevel.AddListener(() =>
                    {
                        if (isCurrentDoor)
                            LockDoor(true, false);
                    });
                    break;
                case LockType.BossDoor:
                    GameManager.OnLockLevel.AddListener(() =>
                    {
                        if (BossKillSave.instance.IsBossKilled(bossName))
                            LockDoor(true, false);
                    });
                    PlayerStateMachine.instance.playerSit.OnSitAtBonfire.AddListener(() =>
                    {
                        if (BossKillSave.instance.IsBossKilled(bossName))
                            LockDoor(true, false);
                        else
                            UnlockDoor(true);
                    });
                    break;
                case LockType.UnlockWhenDefeatingBoss:
                    if (BossKillSave.instance.IsBossKilled(bossName))
                        GameManager.OnChangeScene.AddListener(() => UnlockDoor(true));
                    break;
            }
        }

        private void SetInitialState()
        {
            switch (lockType)
            {
                case LockType.Unlocked:
                    UnlockInstant();
                    break;
                case LockType.LockWhenEnteringRoom:
                case LockType.BossDoor:
                case LockType.UnlockWhenDefeatingBoss:
                    if (isCurrentDoor)
                        UnlockInstant();
                    else
                        LockInstant();
                    break;
            }
        }

        private void LockInstant()
        {
            animator.Play("Closed");
            isLocked = true;
        }

        private void UnlockInstant()
        {
            animator.Play("Opened");
            isLocked = false;
        }

        public void UnlockDoor(bool playSound = false)
        {
            if (lockType == LockType.LockWhenEnteringRoom)
                return;

            if (!isLocked)
                return;

            Sequence.Create()
                .ChainDelay(1.0f)
                .Chain(Tween.Alpha(doorSpriteRenderer, 1.0f, 1.0f, Ease.InCirc))
                .Group(Tween.Alpha(doorwaySpriteRenderer, 1.0f, 1.0f, Ease.InCirc))
                .ChainCallback(() =>
                {
                    if (playSound)
                        SFXManager.instance.PlaySFX(openSound, 0.1f);
                    animator.Play("Unlock");
                    isLocked = false;
                    hitbox.SetActive(isLocked);
                    trigger.gameObject.SetActive(!isLocked);
                });
        }

        public void LockDoor(bool playSound = false, bool makeInvisible = true)
        {
            if (isLocked)
                return;

            animator.Play("Lock");
            isLocked = true;
            hitbox.SetActive(isLocked);
            trigger.gameObject.SetActive(!isLocked);

            if (playSound)
                SFXManager.instance.PlaySFX(closeSound, 0.1f);

            if (makeInvisible)
            {
                Sequence.Create()
                .ChainDelay(1.0f)
                .Chain(Tween.Alpha(doorSpriteRenderer, 0.0f, 1.0f, Ease.OutCirc))
                .Group(Tween.Alpha(doorwaySpriteRenderer, 0.0f, 1.0f, Ease.OutCirc));
            }
        }

        public Vector3 ComputeSpawnPosition()
        {
            Vector3 position = transform.position;
            float distance = 1.5f;

            switch (doorSide)
            {
                case DoorSide.North:
                    position -= Vector3.forward * distance;
                    break;
                case DoorSide.East:
                    position -= Vector3.right * distance;
                    break;
                case DoorSide.South:
                    position += Vector3.forward * distance;
                    break;
                case DoorSide.West:
                    position += Vector3.right * distance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return position;
        }
    }
}
