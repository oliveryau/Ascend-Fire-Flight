using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { PLAY, PAUSE }
    public GameState currentGameState;

    public string currentScene;

    private PlayerController Player;
    private UiManager UiManager;
    private TutorialManager TutorialManager;

    private AudioSource[] allAudioSources;
    private bool playCheckAudio;
    private bool pauseCheckAudio;

    private void Start()
    {
        InitializeManager();
        InitializeEssentials();
    }

    private void Update()
    {
        CheckGameState();
    }

    private void InitializeManager()
    {
        ChangeGameState(GameState.PLAY);
        
        currentScene = SceneManager.GetActiveScene().name;
    }

    public void InitializeEssentials()
    {
        switch (currentScene)
        {
            case "Main Menu":
                Cursor.lockState = CursorLockMode.None;
                AudioManager.Instance.FadeIn("Main Ambience", 2f);
                break;
            case "Main Scene":
            case "Test Scene":
                AudioManager.Instance.FadeIn("Main BGM", 2f);
                AudioManager.Instance.FadeIn("Main Ambience", 2f);

                Player = FindFirstObjectByType<PlayerController>();
                UiManager = FindFirstObjectByType<UiManager>();
                TutorialManager = FindFirstObjectByType<TutorialManager>();
                break;
        }
    }

    #region State Control
    private void CheckGameState()
    {
        if (currentScene != "Main Scene") return;

        switch (currentGameState)
        {
            case GameState.PLAY:
                Time.timeScale = 1f;
                PlayState();
                UnpauseAllAudio();
                break;
            case GameState.PAUSE:
                Time.timeScale = 0f;
                PauseState();
                PauseAllAudio();
                break;
        }
    }

    public void ChangeGameState(GameState newState)
    {
        currentGameState = newState;
    }

    private void PauseAllAudio()
    {
        if (!pauseCheckAudio)
        {
            allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

            foreach (AudioSource audioSource in allAudioSources)
            {
                audioSource.Pause();
            }

            pauseCheckAudio = true;
            playCheckAudio = false;
        }
    }

    private void UnpauseAllAudio()
    {
        if (!playCheckAudio)
        {
            allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

            foreach (AudioSource audioSource in allAudioSources)
            {
                audioSource.UnPause();
            }

            playCheckAudio = true;
            pauseCheckAudio = false;
        }
    }
    #endregion

    #region Play
    private void PlayState()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeGameState(GameState.PAUSE);
            UiManager.ShowPauseMenu(true);
        }
    }

    #endregion

    #region Pause
    private void PauseState()
    {
        Cursor.lockState = CursorLockMode.None;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeGameState(GameState.PLAY);
            UiManager.ShowPauseMenu(false);
        }
    }
    #endregion

    #region Scene Management
    public IEnumerator ReloadMainScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(currentScene);
    }

    public IEnumerator LoadMainMenu()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Main Menu");
    }

    public IEnumerator LoadMainScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Main Scene");
    }
    #endregion
}
