using UnityEngine;

public class DialogueTriggerEvent : MonoBehaviour
{
    [SerializeField] private DialogueLine[] lines;

    private bool hasTriggered = false;
    private DialogueManager DialogueManager;

    private void Start()
    {
        DialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    public void TriggerDialogue()
    {
        if (hasTriggered) return;

        DialogueManager.StartDialogue(lines);
        hasTriggered = true;
    }
}
