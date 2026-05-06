using PrimeTween;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private CinematicBlackBars cinematicBlackBars;
    [SerializeField] private DialogueDisplay dialogueDisplay;
    [SerializeField] private DialoguePortrait portrait;
    [SerializeField] private DialogueName npcName;

    public static DialogueManager instance;

    private void Awake()
    {
        instance = this;
    }

    private bool isOn;

    public void TriggerDialogue(string npc, Sprite npcSprite, DialogueData dialogueData)
    {
        Debug.Log($"Trigger Dialogue : {dialogueData.dialogueLines[0]}");

        isOn = !isOn;

        if (isOn)
        {
            Sequence.Create()
            .ChainCallback(() => cinematicBlackBars.Display())
            .ChainDelay(0.5f)
            .ChainCallback(() => npcName.Display(npc))
            .ChainDelay(0.5f)
            .ChainCallback(() => portrait.Display(npcSprite))
            .ChainDelay(0.5f)
            .ChainCallback(() => dialogueDisplay.Display());
        }
        else
        {
            Sequence.Create()
            .ChainCallback(() => dialogueDisplay.Hide())
            .ChainDelay(0.5f)
            .ChainCallback(() => npcName.Hide())
            .ChainCallback(() => portrait.Hide())
            .ChainDelay(0.5f)
            .ChainCallback(() => cinematicBlackBars.Hide());
        }



    }
}
