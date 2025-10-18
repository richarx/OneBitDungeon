using Interactable;
using Player.Scripts;

namespace Decor.Bonfire
{
    public class Bonfire : InteractableItem
    {
        public override void Interact()
        {
            base.Interact();
            PlayerStateMachine.instance.SitAtBonfire();
        }
    }
}
