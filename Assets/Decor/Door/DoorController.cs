using System;
using Game_Manager;
using PrimeTween;
using SFX;
using Tools_and_Scripts;
using UnityEngine;
using UnityEngine.Assertions;

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

        [SerializeField] private DoorSide doorSide;
        [SerializeField] private bool isSpecialDoor;
        [SerializeField] private string specialTarget;
        [SerializeField] private SceneField targetScene;
        [SerializeField] private bool lockOnEnteringRoom;

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

        public DoorSide doorDirection => doorSide;
        public bool IsSpecialDoor => isSpecialDoor;
        public string SpecialTarget => specialTarget;

        private void Start()
        {
            Assert.IsNotNull(targetScene, $"In Door : {doorSide} => target scene has not been set");

            trigger.OnTrigger.AddListener(() => GameManager.instance.ChangeSceneFromDoor(targetScene.SceneName, this));
            GameManager.OnLockLevel.AddListener(() =>
            {
                if (lockOnEnteringRoom)
                    LockDoor(true, false);
            });
        }

        public void UnlockDoor(bool playSound = false)
        {
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
            float distance = 1.0f;

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
