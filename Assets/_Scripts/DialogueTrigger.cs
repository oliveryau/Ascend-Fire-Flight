using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueLine[] lines;

    private bool hasTriggered = false;
    private DialogueManager DialogueManager;

    private void Start()
    {
        DialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (hasTriggered) return;

        DialogueManager.StartDialogue(lines);
        hasTriggered = true;
    }
}
