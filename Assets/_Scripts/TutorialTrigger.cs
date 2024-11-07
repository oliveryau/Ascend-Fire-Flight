using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    private TutorialManager TutorialManager;

    private bool hasTriggered;

    private void Start()
    {
        TutorialManager = FindFirstObjectByType<TutorialManager>();
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            if (hasTriggered) return;

            hasTriggered = true;
            TutorialManager.tutorialActive = true;
            TutorialManager.currentTutorialState++;
            TutorialManager.needReset = true;
        }
    }
}
