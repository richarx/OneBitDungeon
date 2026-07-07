using System;
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
        [SerializeField] private List<AudioClip> jumpStart;
        [SerializeField] private List<AudioClip> jumpStop;
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
        [SerializeField] private List<AudioClip> outOfBreath;
        [SerializeField] private AudioClip sit_1;
        [SerializeField] private AudioClip sit_2;
        [SerializeField] private AudioClip getUp_1;
        [SerializeField] private AudioClip getUp_2;
        [SerializeField] private AudioClip tagIdle;
        [SerializeField] private AudioClip tagJumpSlam_1;
        [SerializeField] private AudioClip tagJumpSlam_2;
        [SerializeField] private AudioClip tagJumpFlip;

        private PlayerStateMachine player;

        private void Start()
        {
            player = PlayerStateMachine.instance;

            player.playerRoll.OnStartRoll.AddListener(() => SFXManager.instance.PlayRandomSFX(rollStart));
            player.playerRoll.OnStopRoll.AddListener(() => SFXManager.instance.PlayRandomSFX(rollStop, 0.015f));
            player.playerJump.OnStartJump.AddListener(() => SFXManager.instance.PlayRandomSFX(jumpStart));
            player.playerJump.OnLandJump.AddListener(() => SFXManager.instance.PlayRandomSFX(jumpStop, 0.015f));
            player.playerSword.OnEquipSword.AddListener(() => SFXManager.instance.PlaySFX(unsheatheSword, 0.1f));
            player.playerSword.OnSheatheSword.AddListener(() => SFXManager.instance.PlaySFX(sheatheSword, 0.1f));
            player.playerAttack.OnPlayerAttack.AddListener((_) => SFXManager.instance.PlayRandomSFX(swordSlash));
            WeaponDamageTrigger.OnHitEnemy.AddListener((_) => SFXManager.instance.PlayRandomSFX(hitEnemy));
            player.playerHealth.OnPlayerTakeDamage.AddListener((_) =>
            {
                SFXManager.instance.PlayRandomSFX(hurt, delay: 0.05f);
                SFXManager.instance.PlayRandomSFX(hurt_2, 0.05f);
            });
            player.playerParry.OnStartParry.AddListener(() =>
            {
                SFXManager.instance.PlayRandomSFX(parry_1, 0.05f);
                SFXManager.instance.PlaySFX(parry_2, 0.03f);
            });
            player.playerParry.OnSuccessfulParry.AddListener(() =>
            {
                SFXManager.instance.PlaySFX(parrySuccess_1);
                SFXManager.instance.PlaySFX(parrySuccess_2);
            });
            player.playerStamina.OnPlayerExhaustStamina.AddListener(() =>
            {
                float volume = 0.015f;
                SFXManager.instance.PlayRandomSFX(outOfBreath, volume);
                SFXManager.instance.PlayRandomSFX(outOfBreath, volume, 0.5f);
                SFXManager.instance.PlayRandomSFX(outOfBreath, volume, 1.0f);
            });

            player.playerSit.OnStartSittingDown.AddListener(() =>
            {
                SFXManager.instance.PlaySFX(sit_1, 0.1f);
                SFXManager.instance.PlaySFX(sit_2, 0.05f, 0.1f);
            });
            player.playerSit.OnStartGettingUp.AddListener(() =>
            {
                SFXManager.instance.PlaySFX(getUp_1, 0.05f);
                SFXManager.instance.PlaySFX(getUp_2, 0.1f);
            });
            player.playerTag.OnPlayerTag.AddListener(PlayTagSfx);
        }

        private void PlayTagSfx(TagContext tagContext)
        {
            switch (tagContext)
            {
                case TagContext.None:
                    SFXManager.instance.PlaySFX(tagIdle, 0.1f);
                    break;
                case TagContext.Attack:
                    break;
                case TagContext.Roll:
                    break;
                case TagContext.Jump:
                    if (player.playerTagSystem.ActiveSlotIndex == 0)
                    {
                        SFXManager.instance.PlaySFX(tagJumpSlam_1, 0.1f);
                        SFXManager.instance.PlaySFX(tagJumpSlam_2, 0.1f, 0.1f);
                    }
                    else
                        SFXManager.instance.PlaySFX(tagJumpFlip, 0.05f);
                    break;
                case TagContext.SucceededParry:
                    break;
            }
        }
    }
}
