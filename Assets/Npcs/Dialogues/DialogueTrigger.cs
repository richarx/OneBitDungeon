using System.Collections.Generic;
using Interactable;
using UnityEngine;

public class DialogueTrigger : InteractableItem
{
    [SerializeField] private List<DialogueData> dialogueDatas;
    [SerializeField] private Sprite npcSprite;
    [SerializeField] private string npcName;

    public override void Interact()
    {
        base.Interact();
        DialogueManager.instance.TriggerDialogue(npcName, npcSprite, dialogueDatas[Random.Range(0, dialogueDatas.Count)]);
    }
}
