using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialState { MOVEMENT, SPRINT, LAUNCH, SHOOT }
    public TutorialState currentTutorialState;

    public GameObject tutorialCue;

    public bool tutorialActive;

    private void Start()
    {
        currentTutorialState = TutorialState.MOVEMENT;
        tutorialActive = true;
    }

    private void Update()
    {
        ToggleTutorialState();
    }

    private void ToggleTutorialState()
    {
        if (!tutorialActive) return;

        switch (currentTutorialState)
        {
            case TutorialState.MOVEMENT:
            case TutorialState.SPRINT:
            case TutorialState.LAUNCH:
            case TutorialState.SHOOT:
                StartCoroutine(DisplayTutorialCue(currentTutorialState));
                break;
        }
    }

    private IEnumerator DisplayTutorialCue(TutorialState currentState)
    {
        switch (currentState)
        {
            case TutorialState.MOVEMENT:
                tutorialCue.GetComponent<TextMeshProUGUI>().text = "Press WASD to move";
                tutorialCue.SetActive(true);
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.W));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
            case TutorialState.SPRINT:
                tutorialCue.GetComponent<TextMeshProUGUI>().text = "Press Shift to sprint";
                tutorialCue.SetActive(true);
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.LeftShift));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
            case TutorialState.LAUNCH:
                tutorialCue.GetComponent<TextMeshProUGUI>().text = "Press V to launch in the air";
                tutorialCue.SetActive(true);
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.V));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
            case TutorialState.SHOOT:
                tutorialCue.GetComponent<TextMeshProUGUI>().text = "Press the mouse button to kill enemies";
                tutorialCue.SetActive(true);
                yield return new WaitUntil(() => Input.GetButtonDown("Fire1"));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
        }
    }
}
