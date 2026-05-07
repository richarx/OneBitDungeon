using System;
using Player.Scripts;
using PrimeTween;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private CinematicBlackBars cinematicBlackBars;
    [SerializeField] private DialogueDisplay dialogueDisplay;
    [SerializeField] private DialoguePortrait portrait;
    [SerializeField] private DialogueName npcName;

    public static DialogueManager instance;

    private PlayerStateMachine player;

    private bool isDisplayed;
    private DialogueData currentDialogue;
    private int currentLineIndex;
    private string currentLine => currentDialogue.dialogueLines[currentLineIndex];
    private bool IsLastLineOfDialogue => currentLineIndex >= currentDialogue.dialogueLines.Count - 1;

    private bool isSkippingFrame;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        player = PlayerStateMachine.instance;
    }

    private void LateUpdate()
    {
        if (isSkippingFrame)
        {
            isSkippingFrame = false;
            return;
        }

        if (isDisplayed)
        {
            if (player.inputPackage.GetDialogueQuit.wasPressedThisFrame)
                CancelDialogue();
            else if (player.inputPackage.GetDialogueConfirm.wasPressedThisFrame)
            {
                if (dialogueDisplay.IsLineBeingAnimated())
                    dialogueDisplay.InstantlyDisplayLine();
                else if (IsLastLineOfDialogue)
                    CancelDialogue();
                else
                    GoToNextLine();
            }
        }
    }

    private void GoToNextLine()
    {
        currentLineIndex += 1;
        dialogueDisplay.DisplayNewLine(currentLine);
    }

    public void TriggerDialogue(string npc, Sprite npcSprite, DialogueData dialogueData)
    {
        Debug.Log($"Trigger Dialogue : {dialogueData.dialogueLines[0]}");

        if (!isDisplayed)
        {
            isDisplayed = true;
            isSkippingFrame = true;

            currentDialogue = dialogueData;
            currentLineIndex = 0;

            DisplayDialogue(npc, npcSprite);
        }
    }

    public void CancelDialogue()
    {
        if (isDisplayed)
        {
            isDisplayed = false;
            HideDialogue();
        }
    }

    private void DisplayDialogue(string npc, Sprite npcSprite)
    {
        Debug.Log("Display Dialog");
        Sequence.Create()
            .ChainCallback(() => PlayerStateMachine.instance.playerLocked.SetLockState(PlayerStateMachine.instance, PlayerLocked.LockState.Dialog))
            .ChainCallback(() => cinematicBlackBars.Display())
            .ChainDelay(0.5f)
            .ChainCallback(() => npcName.Display(npc))
            .ChainDelay(0.5f)
            .ChainCallback(() => portrait.Display(npcSprite))
            .ChainDelay(0.5f)
            .ChainCallback(() => dialogueDisplay.Display(currentLine));
    }

    private void HideDialogue()
    {
        Debug.Log("Hide Dialog");
        Sequence.Create()
            .ChainCallback(() => dialogueDisplay.Hide())
            .ChainDelay(0.3f)
            .ChainCallback(() => npcName.Hide())
            .ChainCallback(() => portrait.Hide())
            .ChainCallback(() => cinematicBlackBars.Hide())
            .ChainCallback(() => PlayerStateMachine.instance.playerLocked.UnlockPlayer(PlayerStateMachine.instance));
    }
}
