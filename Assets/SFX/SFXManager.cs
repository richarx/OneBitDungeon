using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SFX
{
    public class SFXManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSourcePrefab;
        [SerializeField] private AudioSource audioSourcePrefab3D;
    
        public static SFXManager instance;

        private List<(string channel, float clearChannelTimestamp)> channels = new List<(string, float)>();

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void Update()
        {
            for (int i = channels.Count - 1; i >= 0; i--)
            {
                if (channels[i].clearChannelTimestamp <= Time.time)
                    channels.RemoveAt(i);
            }
        }

        public AudioSource PlaySFXInChannel(string clipChannel, float blockChannelDuration, AudioClip clip, Transform target, float volume = 0.2f, float delay = 0.0f, bool loop = false)
        {
            if (clip == null)
                return null;

            if (channels.Exists((t) => t.channel.Equals(clipChannel)))
                return null;
        
            channels.Add((clipChannel, Time.time + blockChannelDuration));
        
            return PlaySFXAtLocation(clip, target, volume, delay, loop);
        }
        
        public AudioSource PlayRandomSFXInChannel(string clipChannel, float blockChannelDuration, List<AudioClip> clips, Transform target, float volume = 0.2f, float delay = 0.0f, bool loop = false)
        {
            if (clips == null || clips.Count < 1)
                return null;

            if (channels.Exists((t) => t.channel.Equals(clipChannel)))
                return null;
        
            channels.Add((clipChannel, Time.time + blockChannelDuration));
            
            int index = Random.Range(0, clips.Count);
        
            return PlaySFXAtLocation(clips[index], target, volume, delay, loop);
        }
    
        public AudioSource PlayRandomSFX(List<AudioClip> clips, float volume = 0.1f, float delay = 0.0f, bool loop = false)
        {
            if (clips == null || clips.Count < 1)
                return null;
        
            int index = Random.Range(0, clips.Count);

            return PlaySFXAtLocation(clips[index], null, volume, delay, loop);
        }
    
        public AudioSource PlayRandomSFXAtLocation(AudioClip[] clips, Transform target, float volume = 0.1f, float delay = 0.0f, bool loop = false)
        {
            int index = Random.Range(0, clips.Length);

            return PlaySFXAtLocation(clips[index], target, volume, delay, loop);
        }
    
        public AudioSource PlaySFX(AudioClip clip, float volume = 0.2f, float delay = 0.0f, bool loop = false, float pitch = 1.0f)
        {
            if (clip == null)
                return null;
            return PlaySFXAtLocation(clip, null, volume, delay, loop, pitch);
        }

        public AudioSource PlaySFXAtLocation(AudioClip clip, Transform target, float volume = 0.1f, float delay = 0.0f, bool loop = false, float pitch = 1.0f)
        {
            Transform parent = target != null ? target : transform;

            AudioSource source = Instantiate(target != null ? audioSourcePrefab3D : audioSourcePrefab, parent.position, Quaternion.identity, parent);

            source.clip = clip;
            source.volume = volume;
            source.loop = loop;
            source.pitch = pitch + Random.Range(-0.05f, 0.05f);
            if (delay <= 0.0f)
                source.Play();
            else
                source.PlayDelayed(delay);
        
            if (!loop)
                Destroy(source.gameObject, clip.length + delay + 0.15f);

            return source;
        }
    }
}
