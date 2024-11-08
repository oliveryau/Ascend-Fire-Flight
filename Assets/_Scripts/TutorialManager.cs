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
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "WASD - Move";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                break;
            case TutorialState.JUMP:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "Spacebar - Jump";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                break;
            case TutorialState.SPRINT:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "LeftShift - Sprint";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                UiManager.sprintUi.SetActive(true);
                break;
            case TutorialState.LAUNCH:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "V - Fly and Glide";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                UiManager.launchUi.SetActive(true);
                break;
            case TutorialState.SHOOT:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text =
                    "Left Mouse Button - <color=#00FFFF>Blast Palm</color>" +
                    "\n" +
                    "Right Mouse Button - <color=#FFA500>Grenade Launcher</color>";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                UiManager.rightWeaponUi.SetActive(true);
                UiManager.leftWeaponUi.SetActive(true);
                UiManager.ammoUi.SetActive(true);
                UiManager.crosshairUi.SetActive(true);
                UiManager.rightDecorativeUi.SetActive(true);
                break;
            case TutorialState.RELOAD:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "R - Reload <color=#00FFFF>Blast Palm</color>";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                UiManager.reloadUi.SetActive(true);
                break;
            case TutorialState.HEAL:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "E - Restore Health at Healing Crystal";
                tutorialCue.SetActive(true);
                AutoAdvanceTutorial();
                break;
        }
    }
}
