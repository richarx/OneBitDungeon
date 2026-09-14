using Interactable;
using Tutorials;
using UnityEngine;

public class TutorialTrigger : InteractableItem
{
    [SerializeField] private UltraBasicTutorial ultraBasicTutorial;
    [SerializeField] private DialogueData goodbyeDialogue;

    private bool wasTutorialCompleted;

    public override void Interact()
    {
        base.Interact();
        isBeingUsed = true;

        if (wasTutorialCompleted)
            DialogueManager.instance.TriggerDialogue(goodbyeDialogue, null, () => isBeingUsed = false);
        else
            ultraBasicTutorial.LaunchTutorial();
    }

    private void Update()
    {
        if (isBeingUsed && !ultraBasicTutorial.IsExecuting && !wasTutorialCompleted)
        {
            Debug.Log("Stop Being Used");
            isBeingUsed = false;
            wasTutorialCompleted = true;
        }
    }
}
