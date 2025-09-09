using System.Collections.Generic;
using SFX;
using UnityEngine;

namespace Player
{
    public class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private float metersBetweenSteps;
        [SerializeField] private float volume;
        [SerializeField] private List<AudioClip> stepSounds;
        
        private PlayerStateMachine player;

        private float currentMeters;
        private int lastStepSoundPlayed;

        private void Start()
        {
            player = GetComponent<PlayerStateMachine>();
        }

        private void LateUpdate()
        {
            if (IsTimeToTakeStep())
                PlayStepSound(SelectSoundList());
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
            Vector3 horizontalVelocity = player.moveVelocity;
            horizontalVelocity.y = 0.0f;

            currentMeters += horizontalVelocity.magnitude * Time.deltaTime;

            float distance = metersBetweenSteps;
            
            if (currentMeters >= distance)
            {
                currentMeters -= distance;
                return true;
            }

            return false;
        }
    }
}
