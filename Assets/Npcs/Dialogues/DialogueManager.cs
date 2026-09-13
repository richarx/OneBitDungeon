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
    [SerializeField] private DialogueActionButton actionButton;

    public static DialogueManager instance;

    private PlayerStateMachine player;

    private bool isDisplayed;
    private DialogueData currentDialogue;
    private int currentLineIndex;
    private string currentLine => currentDialogue.dialogueLines[currentLineIndex];
    private bool IsLastLineOfDialogue => currentLineIndex >= currentDialogue.dialogueLines.Count - 1;

    private bool isSkippingFrame;

    private Sequence currentSequence;
    private Sequence cameraSequence;

    private Action currentCallback;

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
            else if (player.inputPackage.GetDialogueConfirm.wasPressedThisFrame && dialogueDisplay.isDisplayed)
            {
                if (dialogueDisplay.IsLineBeingAnimated())
                    dialogueDisplay.InstantlyDisplayLine();
                else if (IsLastLineOfDialogue)
                    CancelDialogue();
                else if (dialogueDisplay)
                    GoToNextLine();
            }
        }
    }

    private void GoToNextLine()
    {
        currentLineIndex += 1;
        dialogueDisplay.DisplayNewLine(currentLine);
    }

    public void TriggerDialogue(DialogueData dialogueData, Transform cameraTargetPivot = null, Action callback = null)
    {
        Debug.Log($"Trigger Dialogue : {dialogueData.dialogueLines[0]}");

        if (!isDisplayed)
        {
            currentCallback = callback;
            isDisplayed = true;
            isSkippingFrame = true;

            currentDialogue = dialogueData;
            currentLineIndex = 0;

            if (cameraTargetPivot != null)
                DisplayDialogue(dialogueData.npcName, dialogueData.npcPortrait, cameraTargetPivot.position, cameraTargetPivot.rotation.eulerAngles);
            else
                DisplayDialogue(dialogueData.npcName, dialogueData.npcPortrait, new Vector3(0.0f, 6.0f, -15.0f), new Vector3(20.0f, 0.0f, 0.0f));
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

    private Vector3 cameraStartingPosition;
    private Quaternion cameraStartingRotation;

    private void DisplayDialogue(string npc, Sprite npcSprite, Vector3 cameraPosition, Vector3 cameraRotation)
    {
        Debug.Log("Display Dialog");

        if (currentSequence.isAlive)
            currentSequence.Stop();

        if (cameraSequence.isAlive)
            cameraSequence.Stop();

        Transform camera = CamerasHolder.instance.transform;
        cameraStartingPosition = camera.position;
        cameraStartingRotation = camera.rotation;
        CamerasHolder.instance.cameraFollowPlayer.SetLockState(true);

        cameraSequence = Sequence.Create()
            .Chain(Tween.Position(camera, cameraPosition, 2.0f, Ease.OutCirc))
            .Group(Tween.Rotation(camera, Quaternion.Euler(cameraRotation), 2.0f, Ease.OutCirc));

        currentSequence = Sequence.Create()
            .ChainCallback(() => PlayerStateMachine.instance.playerLocked.SetLockState(PlayerStateMachine.instance, PlayerLocked.LockState.Dialog))
            .ChainCallback(() => cinematicBlackBars.Display(1.0f, Ease.OutCirc, 200.0f))
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

        if (currentSequence.isAlive)
            currentSequence.Stop();

        if (cameraSequence.isAlive)
            cameraSequence.Stop();

        Transform camera = CamerasHolder.instance.transform;

        cameraSequence = Sequence.Create()
            .Chain(Tween.Position(camera, cameraStartingPosition, 1.0f, Ease.InOutQuad))
            .Group(Tween.Rotation(camera, cameraStartingRotation, 1.0f, Ease.InOutQuad));

        currentSequence = Sequence.Create()
            .ChainCallback(() => actionButton.Hide())
            .ChainCallback(() => dialogueDisplay.Hide())
            .ChainDelay(0.3f)
            .ChainCallback(() => npcName.Hide())
            .ChainCallback(() => portrait.Hide())
            .ChainCallback(() => cinematicBlackBars.Hide(0.5f, Ease.InCirc))
            .ChainCallback(() => CamerasHolder.instance.cameraFollowPlayer.SetLockState(true))
            .ChainCallback(() => PlayerStateMachine.instance.playerLocked.UnlockPlayer(PlayerStateMachine.instance))
            .ChainCallback(() =>
            {
                if (currentCallback != null)
                    currentCallback?.Invoke();
            });
    }
}
