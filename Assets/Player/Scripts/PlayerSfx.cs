using System.Collections.Generic;
using Player.Sword_Hitboxes;
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
        [SerializeField] private List<AudioClip> hitEnemy;
        [SerializeField] private List<AudioClip> hurt;
        [SerializeField] private List<AudioClip> hurt_2;
        [SerializeField] private List<AudioClip> parry_1;
        [SerializeField] private AudioClip parry_2;
        [SerializeField] private AudioClip parrySuccess_1;
        [SerializeField] private AudioClip parrySuccess_2;

        private PlayerStateMachine player;
        
        private void Start()
        {
            player = PlayerStateMachine.instance;
            
            player.playerRoll.OnStartRoll.AddListener(() => SFXManager.instance.PlayRandomSFX(rollStart));
            player.playerRoll.OnStopRoll.AddListener(() => SFXManager.instance.PlayRandomSFX(rollStop, 0.015f));
            player.playerSword.OnEquipSword.AddListener(() => SFXManager.instance.PlaySFX(unsheatheSword, 0.1f));
            player.playerSword.OnSheatheSword.AddListener(() => SFXManager.instance.PlaySFX(sheatheSword, 0.1f));
            player.playerAttack.OnPlayerAttack.AddListener((_) => SFXManager.instance.PlayRandomSFX(swordSlash));
            WeaponDamageTrigger.OnHitEnemy.AddListener((_) => SFXManager.instance.PlayRandomSFX(hitEnemy));
            player.playerHealth.OnPlayerTakeDamage.AddListener((_) =>
            {
                SFXManager.instance.PlayRandomSFX(hurt, delay:0.05f);
                SFXManager.instance.PlayRandomSFX(hurt_2, 0.05f);
            });
            player.playerParry.OnParry.AddListener(() =>
            {
                SFXManager.instance.PlayRandomSFX(parry_1, 0.05f);
                SFXManager.instance.PlaySFX(parry_2, 0.03f);
            });
            player.playerParry.OnSuccessfulParry.AddListener(() =>
            {
                SFXManager.instance.PlaySFX(parrySuccess_1);
                SFXManager.instance.PlaySFX(parrySuccess_2);
            });
        }
    }
}
