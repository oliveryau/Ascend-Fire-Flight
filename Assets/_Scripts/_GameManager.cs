using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { WAIT, STOP, PLAY, PAUSE }
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
                AudioManager.Instance.FadeIn("Main Ambience", 2f); //Menu bgm
                break;
            case "Main Scene":
                AudioManager.Instance.FadeIn("Main BGM", 2f);

                Player = FindFirstObjectByType<PlayerController>();
                UiManager = FindFirstObjectByType<UiManager>();
                TutorialManager = FindFirstObjectByType<TutorialManager>();
                break;
            case "Ending":
                Cursor.lockState = CursorLockMode.None;
                AudioManager.Instance.FadeIn("Main BGM", 2f); //Creds bgm
                StartCoroutine(EndingSequence());
                break;
        }
    }

    private IEnumerator EndingSequence()
    {
        yield return new WaitForSeconds(30f);
        StartCoroutine(FadeToggle(false, "Main Menu"));
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
            case GameState.STOP:
                Time.timeScale = 0f;
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
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("Main Scene");
    }

    public void LoadEnding()
    {
        SceneManager.LoadScene("Ending");
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
            yield return new WaitForSeconds(0.1f);
            ChangeGameState(GameState.PLAY);
            fadePrefab.SetActive(false);
        }
        else
        {
            fadePrefab.GetComponent<Animator>().SetTrigger("Fade Out");
            ChangeGameState(GameState.WAIT);
            yield return new WaitForSeconds(1f);

            if (sceneName == "Main Scene")
            {
                LoadMainScene();
            }
            else if (sceneName == "Main Menu")
            {
                LoadMainMenu();
            }
            else if (sceneName == "Ending")
            {
                LoadEnding();
            }
        }
    }
    #endregion
}
