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
        AudioManager.Instance.PlayOneShot("UI Click", gameObject);
        StartCoroutine(GameManager.FadeToggle(false, "Main Scene")); //Fade out, main menu
    }

    public void GoToSettings()
    {

    }

    public void QuitGame()
    {
        AudioManager.Instance.PlayOneShot("UI Click", gameObject);
        Application.Quit();
    }

    public void ButtonHoverSound()
    {
        AudioManager.Instance.PlayOneShot("UI Hover", gameObject);
    }
}
