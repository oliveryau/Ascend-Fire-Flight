using UnityEngine;

public class _UiManager : MonoBehaviour
{
    public GameObject pauseMenu;

    private _GameManager GameManager;

    private void Start()
    {
        GameManager = FindFirstObjectByType<_GameManager>();
    }

    public void ShowPauseMenu(bool activeMode)
    {
        if (activeMode)
        {
            pauseMenu.SetActive(true);
        }
        else
        {
            pauseMenu.SetActive(false);
        }
    }
}
