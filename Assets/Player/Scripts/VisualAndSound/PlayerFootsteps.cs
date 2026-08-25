using System.Collections.Generic;
using SFX;
using Tools_and_Scripts;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private float metersBetweenSteps;
        [SerializeField] private float metersBetweenArrogantSteps;
        [SerializeField] private float volume;
        [SerializeField] private List<AudioClip> stepSounds;
        [SerializeField] private List<GameObject> stepPrefabs;

        private PlayerStateMachine player;

        private float currentMeters;
        private int lastStepSoundPlayed;

        private void Start()
        {
            player = GetComponent<PlayerStateMachine>();

            player.playerAttack.OnPlayerAttack.AddListener((payload) =>
            {
                if (payload.Type == AttackType.Light)
                    SpawnStepVfx();
            });
            player.playerArrogantSpin.OnStartSpin.AddListener(() => SpawnStepVfx());
        }

        private void LateUpdate()
        {
            if (IsTimeToTakeStep())
            {
                PlayStepSound(SelectSoundList());
                SpawnStepVfx();
            }
        }

        private void SpawnStepVfx()
        {
            int index = Random.Range(0, stepPrefabs.Count);

            GameObject step = Instantiate(stepPrefabs[index], player.position, Quaternion.identity);
            step.UseUnscaledTime();

            if (player.moveVelocity.x > 0.0f)
                step.GetComponent<SpriteRenderer>().flipX = true;
        }

        private List<AudioClip> SelectSoundList()
        {
            return stepSounds;
        }

        private void PlayStepSound(List<AudioClip> soundList)
        {
            if (soundList.Count < 2)
            {
                if (soundList.Count > 0)
                    SFXManager.instance.PlaySFX(soundList[0], 0.03f);
                return;
            }

            int previousSoundIndex = lastStepSoundPlayed;

            int randomIndex = Random.Range(0, soundList.Count);

            if (randomIndex == previousSoundIndex)
                randomIndex = randomIndex == soundList.Count - 1 ? 0 : randomIndex + 1;

            SFXManager.instance.PlaySFX(soundList[randomIndex], volume);

            lastStepSoundPlayed = randomIndex;
        }

        private bool IsTimeToTakeStep()
        {
            BehaviourType currentBehaviour = player.currentBehaviour.GetBehaviourType();

            if (!IsBehaviourAllowed(currentBehaviour))
                return false;

            Vector3 horizontalVelocity = player.moveVelocity;
            horizontalVelocity.y = 0.0f;

            currentMeters += horizontalVelocity.magnitude * Time.deltaTime;

            bool isArrogantWalking = currentBehaviour == BehaviourType.ArrogantRun;
            float distance = isArrogantWalking ? metersBetweenArrogantSteps : metersBetweenSteps;

            if (currentMeters >= distance)
            {
                currentMeters -= distance;
                return true;
            }

            return false;
        }

        private bool IsBehaviourAllowed(BehaviourType behaviour)
        {
            return behaviour == BehaviourType.Run || behaviour == BehaviourType.ArrogantRun;
        }
    }
}
