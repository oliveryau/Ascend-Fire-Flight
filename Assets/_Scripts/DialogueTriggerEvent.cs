using UnityEngine;

public class DialogueTriggerEvent : MonoBehaviour
{
    [SerializeField] private DialogueLine[] lines;
    [SerializeField] private bool allowRepeating;

    private bool hasTriggered = false;
    private DialogueManager DialogueManager;

    private void Start()
    {
        DialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    public void TriggerDialogue()
    {
        if (!allowRepeating && hasTriggered) return;

        DialogueManager.StartDialogue(lines);
        hasTriggered = true;
    }
}
