using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 10)]
    public string text;
    public string soundClipName;
}

public class DialogueManager : MonoBehaviour
{
    private DialogueLine[] currentLines;
    private int currentLineIndex = -1;
    private bool isDialogueActive = false;

    private float lineStartTime;
    private float autoAdvanceDelay = 4f;

    private UiManager UiManager;

    private void Start()
    {
        UiManager = GetComponent<UiManager>();
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        bool shouldAdvance = Time.time - lineStartTime >= autoAdvanceDelay;
        if (shouldAdvance) DisplayNextLine();
    }

    public void StartDialogue(DialogueLine[] lines)
    {
        currentLines = lines;
        currentLineIndex = -1;
        isDialogueActive = true;
        UiManager.dialoguePanel.SetActive(true);
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentLines.Length)
        {
            DialogueLine currentLine = currentLines[currentLineIndex];
            UiManager.dialogueUi.GetComponent<Animator>().SetTrigger("Nextline");
            UiManager.dialogueText.text = currentLine.text;
            if (!string.IsNullOrEmpty(currentLine.soundClipName)) AudioManager.Instance.PlayOneShot(currentLine.soundClipName, gameObject);
            lineStartTime = Time.time;
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        UiManager.dialoguePanel.SetActive(false);
    }
}