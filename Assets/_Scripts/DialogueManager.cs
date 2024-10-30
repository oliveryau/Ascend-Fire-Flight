using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 10)]
    public string text;
}

public class DialogueManager : MonoBehaviour
{
    private DialogueLine[] currentLines;
    private int currentLineIndex = -1;
    private bool isDialogueActive = false;

    private float lineStartTime;
    private float autoAdvanceDelay = 5f;

    private UiManager UiManager;

    private void Start()
    {
        UiManager = GetComponent<UiManager>();
    }

    private void Update()
    {
        if (isDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                DisplayNextLine();
            }
            else if (Time.time - lineStartTime >= autoAdvanceDelay)
            {
                DisplayNextLine();
            }
        }
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
            UiManager.dialogueText.text = currentLines[currentLineIndex].text;
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