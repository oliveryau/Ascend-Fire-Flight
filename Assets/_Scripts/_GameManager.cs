using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { PLAY, PAUSE }
    public GameState currentGameState;

    private PlayerController Player;
    private UiManager UiManager;
    private TutorialManager TutorialManager;

    private void Start()
    {
        ChangeGameState(GameState.PLAY);

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
                break;
            case GameState.PAUSE:
                Time.timeScale = 0f;
                PauseState();
                break;
        }
    }

    public void ChangeGameState(GameState newState)
    {
        currentGameState = newState;
    }
    #endregion

    #region Play
    private void PlayState()
    {
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeGameState(GameState.PLAY);
            UiManager.ShowPauseMenu(false);
        }
    }

    public void ResumeGame()
    {

    }

    public void RestartGame()
    {
        //Go to last checkpoint or restart game
    }

    public void Settings()
    {

    }

    public void ResetTutorial()
    {
        //Reload scene
        Player.initialPosition = new Vector3(0, 1, 0); //Back to start position
        TutorialManager.currentTutorialState = TutorialManager.TutorialState.MOVEMENT;
    }

    public void ReturnToMainMenu()
    {

    }
    #endregion
}
