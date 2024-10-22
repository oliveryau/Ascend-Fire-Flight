using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private GameManager GameManager;

    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();
    }

    public void StartGame()
    {
        //StartCoroutine(FadeToggle(false, false)); //Fade out, main menu
    }

    public void GoToSettings()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
