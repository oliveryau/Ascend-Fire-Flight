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
        ChangeGameState(GameState.PLAY);
        currentScene = SceneManager.GetActiveScene().name;

        Player = FindFirstObjectByType<PlayerController>();
        UiManager = FindFirstObjectByType<UiManager>();
        TutorialManager = FindFirstObjectByType<TutorialManager>();
    }

    private void Update()
    {
        CheckGameState();
    }

    #region State Control
    private void CheckGameState()
    {
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
    public IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(currentScene);
    }
    #endregion
}
