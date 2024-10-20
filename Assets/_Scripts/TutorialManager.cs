using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialState { MOVEMENT, JUMP, SPRINT, LAUNCH, SHOOT, RELOAD, HEAL, END }
    public TutorialState currentTutorialState;

    public bool tutorialActive;

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
                StartCoroutine(DisplayTutorialCue(currentTutorialState));
                break;
            case TutorialState.END:
                tutorialActive = false;
                break;
        }
    }

    private IEnumerator DisplayTutorialCue(TutorialState currentState)
    {
        switch (currentState)
        {
            case TutorialState.MOVEMENT:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "Press WASD to move";
                tutorialCue.SetActive(true);
                yield return new WaitUntil(() => Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical"));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
            case TutorialState.JUMP:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "Press Spacebar to jump";
                tutorialCue.SetActive(true);
                yield return new WaitUntil(() => Input.GetButtonDown("Jump"));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
            case TutorialState.SPRINT:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "Press LeftShift to sprint";
                tutorialCue.SetActive(true);
                UiManager.sprintUi.SetActive(true);
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.LeftShift));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
            case TutorialState.LAUNCH:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "Press and hold V to fly and glide in the air";
                tutorialCue.SetActive(true);
                UiManager.launchUi.SetActive(true);
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.V));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
            case TutorialState.SHOOT:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "Press Left Mouse Button to fire blast palm\nPress Right Mouse Button to fire grenade launcher";
                tutorialCue.SetActive(true);
                UiManager.rightWeaponUi.SetActive(true);
                UiManager.leftWeaponUi.SetActive(true);
                UiManager.ammoUi.SetActive(true);
                UiManager.crosshairUi.SetActive(true);
                UiManager.rightDecorativeUi.SetActive(true);
                yield return new WaitUntil(() => Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2"));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
            case TutorialState.RELOAD:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "Press R to reload";
                tutorialCue.SetActive(true);
                UiManager.reloadUi.SetActive(true);
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.R));
                tutorialCue.SetActive(false);
                tutorialActive = false;
                break;
            case TutorialState.HEAL:
                tutorialCue.GetComponentInChildren<TextMeshProUGUI>().text = "Press E to heal";
                tutorialCue.SetActive(true);
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
                tutorialCue.SetActive(false);
                currentTutorialState = TutorialState.END;
                break;
        }
    }
}
