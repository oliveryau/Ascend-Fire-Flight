using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialState { MOVEMENT, JUMP, SPRINT, LAUNCH, SHOOT, RELOAD, HEAL }
    public TutorialState currentTutorialState;

    public bool tutorialActive;
    public bool needReset;

    private float advanceStartTime;
    private float autoAdvanceDelay = 10f;

    private UiManager UiManager;
    private GameObject tutorialCue;

    private void Start()
    {
        currentTutorialState = TutorialState.MOVEMENT;

        UiManager = FindFirstObjectByType<UiManager>();
        tutorialCue = FindFirstObjectByType<UiManager>().tutorialCue;
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
            case TutorialState.JUMP:
            case TutorialState.SPRINT:
            case TutorialState.LAUNCH:
            case TutorialState.SHOOT:
            case TutorialState.RELOAD:
            case TutorialState.HEAL:
                DisplayNextCue(currentTutorialState);
                break;
        }
    }

    private void AutoAdvanceTutorial()
    {
        if (!tutorialActive) return;

        bool shouldAdvance = Time.time - advanceStartTime >= autoAdvanceDelay;
        if (shouldAdvance)
        {
            tutorialCue.SetActive(false);
            tutorialActive = false;
            needReset = true;
        }
    }

    private void DisplayNextCue(TutorialState currentState)
    {
        if (needReset)
        {
            advanceStartTime = Time.time;
            tutorialCue.GetComponent<Animator>().SetTrigger("Spawn");
            needReset = false;
        }

        switch (currentState)
        {
            case TutorialState.MOVEMENT:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "<b>WASD</b> - Move";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                break;
            case TutorialState.JUMP:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "<b>Spacebar</b> - Jump";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                break;
            case TutorialState.SPRINT:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "<b>LeftShift</b> - Sprint";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                UiManager.sprintUi.SetActive(true);
                break;
            case TutorialState.LAUNCH:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "<b>V</b> - <color=#FFA500>Fly and Glide</color>";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                UiManager.launchUi.SetActive(true);
                break;
            case TutorialState.SHOOT:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text =
                    "<b>LMB</b> - <color=#00FFFF>Blast Palm</color>" +
                    "\n" +
                    "<b>RMB</b> - <color=#FFA500>Grenade Launcher</color>";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                UiManager.rightWeaponUi.SetActive(true);
                UiManager.leftWeaponUi.SetActive(true);
                UiManager.ammoUi.SetActive(true);
                UiManager.crosshairUi.SetActive(true);
                UiManager.rightDecorativeUi.SetActive(true);
                break;
            case TutorialState.RELOAD:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "<b>R</b> - Reload <color=#00FFFF>Blast Palm</color>";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                UiManager.reloadUi.SetActive(true);
                break;
            case TutorialState.HEAL:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "<b>E</b> - <color=#00FF00>Heal</color> at Rejuvenating Crystal";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                break;
        }
    }
}
