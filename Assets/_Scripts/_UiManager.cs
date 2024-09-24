using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject pauseMenu;
    public GameObject tutorialCue;

    [Header("Player UI")]
    public float updateSpeed;
    public Image playerHealthFill;

    private float targetHealthFill;

    [Header("Diegetic UI")]
    public Transform playerLaunchMeter;
    public GameObject[] playerHealCharge;
    public TextMeshProUGUI playerAmmoCount;

    private Renderer launchMeterRenderer;

    [Header("Other UI")]
    public Image playerFallingOutOfBoundsOverlay;
    public Image playerDamagedOverlay;

    [Header("References")]
    private GameManager GameManager;
    private PlayerController Player;

    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();
        Player = FindFirstObjectByType<PlayerController>();

        InitializeUiValues();
    }

    private void Update()
    {
        UpdatePlayerHealthBar();
        UpdatePlayerLaunchMeter();
        UpdatePlayerHealCharge();
    }

    private void InitializeUiValues()
    {
        targetHealthFill = Player.currentHealth / Player.maxHealth;
        playerHealthFill.fillAmount = targetHealthFill;

        launchMeterRenderer = playerLaunchMeter.GetComponent<Renderer>();
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

    #region Player UI
    public void UpdatePlayerHealthBar()
    {
        targetHealthFill = Player.currentHealth / Player.maxHealth;
        playerHealthFill.fillAmount = Mathf.Lerp(playerHealthFill.fillAmount, targetHealthFill, Time.deltaTime * updateSpeed);

        Color lerpedColor = Color.Lerp(Color.red, Color.green, playerHealthFill.fillAmount);
        playerHealthFill.color = lerpedColor;
    }

    public void UpdatePlayerLaunchMeter()
    {
        float fillPercentage = Player.currentLaunchMeter / Player.launchMeter;
        Vector3 currentScale = playerLaunchMeter.localScale;
        Vector3 targetScale = new Vector3(Mathf.Lerp(0, 1, fillPercentage), currentScale.y, currentScale.z);

        playerLaunchMeter.localScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * updateSpeed);

        Color lerpedColor = Color.Lerp(Color.gray, Color.cyan, fillPercentage);
        launchMeterRenderer.material.color = lerpedColor;
    }

    public void UpdatePlayerHealCharge()
    {
        for (int i = 0; i < playerHealCharge.Length; i++)
        {
            Renderer healChargeRenderer = playerHealCharge[i].GetComponent<Renderer>();

            if (i < Player.currentHealCharge) healChargeRenderer.material.color = Color.green;
            else healChargeRenderer.material.color = Color.gray;       
        }
    }

    public void UpdatePlayerAmmoCount()
    {
        if (Player.currentAmmo <= 0)
        {
            playerAmmoCount.color = Color.red;
            playerAmmoCount.text = "R";
        }
        else
        {
            if (playerAmmoCount.color == Color.red) playerAmmoCount.color = Color.white;
            playerAmmoCount.text = Player.currentAmmo.ToString();
        }
    }
    #endregion

    #region Other UI
    public IEnumerator DisplayFallingOutOverlay()
    {
        playerFallingOutOfBoundsOverlay.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        playerFallingOutOfBoundsOverlay.gameObject.SetActive(false);
    }

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
    #endregion
}
