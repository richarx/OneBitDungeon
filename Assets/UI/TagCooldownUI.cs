using Player.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TagCooldownUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        private PlayerStateMachine player;

        private void Start()
        {
            player = PlayerStateMachine.instance;
        }

        private void Update()
        {
            var tagSystem = player.playerTagSystem;
            fillImage.fillAmount = tagSystem != null ? tagSystem.TagCooldownProgress : 1f;
        }
    }
}
