using System.Collections.Generic;
using Interactable;
using UnityEngine;

public class DialogueTrigger : InteractableItem
{
    enum DialogueSelection
    {
        Random,
        Chain,
        ChainAndRepeatLast
    }

    [SerializeField] private List<DialogueData> dialogueDatas;
    [SerializeField] private DialogueSelection dialogueSelection;
    [SerializeField] private Sprite npcSprite;
    [SerializeField] private string npcName;
    [SerializeField] private Transform cameraTargetPivot;

    private int currentDialogueIndex = 0;

    protected override void Start()
    {
        base.Start();
        detection.OnPlayerExitRange.AddListener(() => DialogueManager.instance.CancelDialogue());
    }

    public override void Interact()
    {
        base.Interact();
        isBeingUsed = true;
        DialogueManager.instance.TriggerDialogue(npcName, npcSprite, ChooseDialogue(), cameraTargetPivot);
    }

    private DialogueData ChooseDialogue()
    {
        if (dialogueDatas.Count == 1)
            return dialogueDatas[0];

        DialogueData dialogue = dialogueDatas[0];

        switch (dialogueSelection)
        {
            case DialogueSelection.Random:
                dialogue = dialogueDatas[Random.Range(0, dialogueDatas.Count)];
                break;
            case DialogueSelection.Chain:
                dialogue = dialogueDatas[currentDialogueIndex];
                if (currentDialogueIndex < dialogueDatas.Count - 1)
                    currentDialogueIndex += 1;
                else
                    currentDialogueIndex = 0;
                break;
            case DialogueSelection.ChainAndRepeatLast:
                dialogue = dialogueDatas[currentDialogueIndex];
                if (currentDialogueIndex < dialogueDatas.Count - 1)
                    currentDialogueIndex += 1;
                break;
        }

        return dialogue;
    }
}
