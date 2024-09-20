using UnityEngine;

public class UiManager : MonoBehaviour
{
    [Header("Diegetic UI")]
    public Transform meterFillTransform;
    public float maxMeterScale;
    public Color fullColor;
    public Color emptyColor;

    [Header("Main UI")]
    public GameObject pauseMenu;
    public GameObject tutorialCue;

    private Renderer meterRenderer;

    private GameManager GameManager;
    private PlayerController Player;

    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();
        Player = FindFirstObjectByType<PlayerController>();

        meterRenderer = meterFillTransform.GetComponent<Renderer>();
    }

    private void Update()
    {
        UpdateMeterVisual();
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

    private void UpdateMeterVisual()
    {
        float fillPercentage = Player.currentLaunchMeter / Player.launchMeter;

        // Update the scale of the meter fill
        Vector3 currentScale = meterFillTransform.localScale;
        currentScale.x = Mathf.Lerp(0, maxMeterScale, fillPercentage);
        meterFillTransform.localScale = currentScale;

        // Update the color of the meter
        Color lerpedColor = Color.Lerp(emptyColor, fullColor, fillPercentage);
        meterRenderer.material.color = lerpedColor;
    }
}
