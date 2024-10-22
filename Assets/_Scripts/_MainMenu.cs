using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private GameManager GameManager;

    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();

        StartCoroutine(GameManager.FadeToggle(true)); //Fade in
    }

    public void StartGame()
    {
        StartCoroutine(GameManager.FadeToggle(false, "Main Scene")); //Fade out, main menu
    }

    public void GoToSettings()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
