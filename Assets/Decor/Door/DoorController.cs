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
        [SerializeField] private SceneField targetScene;
        [SerializeField] private Animator animator;
        [SerializeField] private DoorTrigger trigger;
        [SerializeField] private GameObject hitbox;
        [SerializeField] private SpriteRenderer doorSpriteRenderer;
        [SerializeField] private SpriteRenderer doorwaySpriteRenderer;

        private bool isLocked;

        public DoorSide doorDirection => doorSide;

        private void Start()
        {
            Assert.IsNotNull(targetScene, $"In Door : {doorSide} => target scene has not been set");

            trigger.OnTrigger.AddListener(() => GameManager.instance.ChangeScene(targetScene.SceneName, doorSide.Opposite()));
        }

        public void UnlockDoor()
        {
            Sequence.Create()
                .ChainDelay(1.0f)
                .Chain(Tween.Alpha(doorSpriteRenderer, 1.0f, 1.0f, Ease.InCirc))
                .Group(Tween.Alpha(doorwaySpriteRenderer, 1.0f, 1.0f, Ease.InCirc))
                .ChainCallback(() =>
                {
                    animator.Play("Unlock");
                    isLocked = false;
                    hitbox.SetActive(isLocked);
                    trigger.gameObject.SetActive(!isLocked);
                });
        }

        public void LockDoor()
        {
            animator.Play("Lock");
            isLocked = true;
            hitbox.SetActive(isLocked);
            trigger.gameObject.SetActive(!isLocked);

            Sequence.Create()
                .ChainDelay(1.0f)
                .Chain(Tween.Alpha(doorSpriteRenderer, 0.0f, 1.0f, Ease.OutCirc))
                .Group(Tween.Alpha(doorwaySpriteRenderer, 0.0f, 1.0f, Ease.OutCirc));
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
                    position += Vector3.right * distance;
                    break;
                case DoorSide.South:
                    position += Vector3.forward * distance;
                    break;
                case DoorSide.West:
                    position -= Vector3.right * distance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return position;
        }
    }
}
