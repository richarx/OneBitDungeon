using System.Collections.Generic;
using SFX;
using UnityEngine;

namespace Player.Scripts
{
    public class PlayerSfx : MonoBehaviour
    {
        [SerializeField] private List<AudioClip> rollStart;
        [SerializeField] private List<AudioClip> rollStop;
        [SerializeField] private AudioClip sheatheSword;
        [SerializeField] private AudioClip unsheatheSword;
        [SerializeField] private List<AudioClip> swordSlash;

        private PlayerStateMachine player;
        
        private void Start()
        {
            player = PlayerStateMachine.instance;
            
            player.playerRoll.OnStartRoll.AddListener(() => SFXManager.instance.PlayRandomSFX(rollStart));
            player.playerRoll.OnStopRoll.AddListener(() => SFXManager.instance.PlayRandomSFX(rollStop, 0.015f));
            player.playerSword.OnEquipSword.AddListener(() => SFXManager.instance.PlaySFX(unsheatheSword, 0.1f));
            player.playerSword.OnSheatheSword.AddListener(() => SFXManager.instance.PlaySFX(sheatheSword, 0.1f));
            player.playerAttack.OnPlayerAttack.AddListener((_) => SFXManager.instance.PlayRandomSFX(swordSlash));
        }
    }
}
