using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { WAIT, PLAY, PAUSE }
    public GameState currentGameState;

    public string currentScene;

    public GameObject fadePrefab;

    private PlayerController Player;
    private UiManager UiManager;
    private TutorialManager TutorialManager;

    private AudioSource[] allAudioSources;
    private bool playCheckAudio;
    private bool pauseCheckAudio;

    #region Initialization
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
    #endregion

    #region State Control
    private void CheckGameState()
    {
        if (currentScene != "Main Scene") return;

        switch (currentGameState)
        {
            case GameState.WAIT:
                Time.timeScale = 1f;
                break;
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

    #region Fading
    public IEnumerator FadeToggle(bool fadeIn, string sceneName = null)
    {
        fadePrefab.SetActive(true);

        if (fadeIn)
        {
            fadePrefab.GetComponent<Animator>().SetTrigger("Fade In");
            ChangeGameState(GameState.WAIT);
            yield return new WaitForSeconds(1f);
            ChangeGameState(GameState.PLAY);
            fadePrefab.SetActive(false);
        }
        else if (!fadeIn && sceneName == "Main Scene")
        {
            fadePrefab.GetComponent<Animator>().SetTrigger("Fade Out");
            ChangeGameState(GameState.WAIT);
            yield return new WaitForSeconds(1f);
            StartCoroutine(LoadMainScene());
        }
        else if (!fadeIn && sceneName == "Main Menu")
        {
            fadePrefab.GetComponent<Animator>().SetTrigger("Fade Out");
            ChangeGameState(GameState.WAIT);
            yield return new WaitForSeconds(1f);
            StartCoroutine(LoadMainMenu());
        }
    }
    #endregion
}
