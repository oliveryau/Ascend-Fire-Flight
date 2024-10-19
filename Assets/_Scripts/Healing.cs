using UnityEngine;

public class Healing : MonoBehaviour
{
    private UiManager UiManager;

    private void Start()
    {
        UiManager = FindFirstObjectByType<UiManager>();
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            UiManager.ToggleHealCue(true);
        }
    }

    private void OnTriggerExit(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            UiManager.ToggleHealCue(false);
        }
    }
}
