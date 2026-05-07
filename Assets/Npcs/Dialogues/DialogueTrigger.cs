using System.Collections.Generic;
using Interactable;
using UnityEngine;

public class DialogueTrigger : InteractableItem
{
    [SerializeField] private List<DialogueData> dialogueDatas;
    [SerializeField] private Sprite npcSprite;
    [SerializeField] private string npcName;
    [SerializeField] private Transform cameraTargetPivot;

    protected override void Start()
    {
        base.Start();
        detection.OnPlayerExitRange.AddListener(() => DialogueManager.instance.CancelDialogue());
    }

    public override void Interact()
    {
        base.Interact();
        isBeingUsed = true;
        DialogueManager.instance.TriggerDialogue(npcName, npcSprite, dialogueDatas[Random.Range(0, dialogueDatas.Count)], cameraTargetPivot);
    }
}
