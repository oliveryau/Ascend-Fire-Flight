using UnityEngine;

public class _GameManager : MonoBehaviour
{
    public enum GameState { PLAY, PAUSE }
    public GameState currentGameState;

    private _UiManager UiManager;

    private void Start()
    {
        ChangeGameState(GameState.PLAY);

        UiManager = FindFirstObjectByType<_UiManager>();
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
    #endregion
}
