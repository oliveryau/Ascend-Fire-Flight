using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject pauseMenu;
    public GameObject tutorialCue;

    [Header("Player UI")]
    public Image playerHealthFill;
    public float healthUpdateSpeed;

    //public Transform launchMeterTransform;
    public Color fullColor;
    public Color emptyColor;

    private float targetHealthFill;
    //private Renderer launchMeterRenderer;

    [Header("Other UI")]
    public Image playerDamagedOverlay;

    [Header("References")]
    private GameManager GameManager;
    private PlayerController Player;

    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();
        Player = FindFirstObjectByType<PlayerController>();

        //launchMeterRenderer = launchMeterTransform.GetComponent<Renderer>();

        targetHealthFill = Player.currentHealth / Player.maxHealth;
        playerHealthFill.fillAmount = targetHealthFill;
    }

    private void Update()
    {
        UpdatePlayerHealthBar();
        //UpdateLaunchMeter();
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

    private void UpdatePlayerHealthBar()
    {
        targetHealthFill = Player.currentHealth / Player.maxHealth;
        playerHealthFill.fillAmount = Mathf.Lerp(playerHealthFill.fillAmount, targetHealthFill, Time.deltaTime * healthUpdateSpeed);

        Color lerpedColor = Color.Lerp(emptyColor, fullColor, playerHealthFill.fillAmount);
        playerHealthFill.color = lerpedColor;
    }

    //private void UpdateLaunchMeter()
    //{
    //    float fillPercentage = Player.currentLaunchMeter / Player.launchMeter;

    //    Vector3 currentScale = launchMeterTransform.localScale;
    //    currentScale.x = Mathf.Lerp(0, 1, fillPercentage);
    //    launchMeterTransform.localScale = currentScale;

    //    Color lerpedColor = Color.Lerp(emptyColor, fullColor, fillPercentage);
    //    launchMeterRenderer.material.color = lerpedColor;
    //}

    public void DisplayDamagedOverlay()
    {
        StartCoroutine(DamagedOverlay());
    }

    private IEnumerator DamagedOverlay()
    {
        playerDamagedOverlay.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        playerDamagedOverlay.gameObject.SetActive(false);
    }
}
