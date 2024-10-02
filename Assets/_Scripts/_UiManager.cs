using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    #region Variables
    [Header("Main UI")]
    public GameObject pauseMenu;
    public GameObject tutorialCue;

    [Header("Player UI")]
    public float updateSpeed;
    public Image playerHealthFill;
    public GameObject rightCrosshair;
    public GameObject leftCrosshair;
    public float crosshairDetectionRange;

    private float targetHealthFill;
    private Animator rightCrosshairAnimator;
    private Animator leftCrosshairAnimator;
    private bool rightCrosshairOnEnemy;

    [Header("Diegetic UI")]
    public Transform playerLaunchMeter;
    public GameObject[] playerHealCharge;
    public TextMeshProUGUI playerAmmoCount;

    private Renderer launchMeterRenderer;

    [Header("Enemy Indicator UI")]
    public GameObject enemyIndicatorPrefab;
    public float enemyDetectionRadius;
    public float indicatorDistance; //Distance from screen center (0-1)
    public Transform indicatorParent;
    public LayerMask enemyLayer;

    private Dictionary<GameObject, GameObject> enemyIndicators = new Dictionary<GameObject, GameObject>();
    private const float RIGHT_ROTATION = -90f;
    private const float LEFT_ROTATION = 90f;
    private const float TOP_ROTATION = 180f;
    private const float BOTTOM_ROTATION = 0f;

    [Header("Other UI")]
    public GameObject fadePrefab;
    public Image playerFallingOutOfBoundsOverlay;
    public Image playerDamagedOverlay;

    [Header("References")]
    private GameManager GameManager;
    private PlayerController Player;
    private Camera MainCamera;
    private TutorialManager TutorialManager;
    #endregion

    #region UI Initialization
    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();
        Player = FindFirstObjectByType<PlayerController>();
        MainCamera = Camera.main;

        InitializeUi();
    }

    private void Update()
    {
        if (Player.currentPlayerState != PlayerController.PlayerState.DEAD)
        {
            UpdatePlayerHealthBar();
            UpdatePlayerLaunchMeter();
            UpdatePlayerHealCharge();
            UpdateRightCrosshairDetection();
            UpdateEnemyDetection();
            UpdateIndicatorPositions();
        }
    }

    private void InitializeUi()
    {
        StartCoroutine(FadeToggle(true)); //Fade in

        targetHealthFill = Player.currentHealth / Player.maxHealth;
        playerHealthFill.fillAmount = targetHealthFill;

        launchMeterRenderer = playerLaunchMeter.GetComponent<Renderer>();

        UpdatePlayerAmmoCount();

        rightCrosshairAnimator = rightCrosshair.GetComponent<Animator>();
        leftCrosshairAnimator = leftCrosshair.GetComponent<Animator>();
    }
    #endregion

    #region Player UI
    public void UpdatePlayerHealthBar()
    {
        targetHealthFill = Player.currentHealth / Player.maxHealth;
        playerHealthFill.fillAmount = Mathf.Lerp(playerHealthFill.fillAmount, targetHealthFill, Time.deltaTime * updateSpeed);

        Color lerpedColor = Color.Lerp(Color.red, Color.white, playerHealthFill.fillAmount);
        playerHealthFill.color = lerpedColor;
    }

    public void UpdatePlayerLaunchMeter()
    {
        float fillPercentage = Player.currentLaunchMeter / Player.maxLaunchMeter;
        Vector3 currentScale = playerLaunchMeter.localScale;
        Vector3 targetScale = new Vector3(Mathf.Lerp(0, 1, fillPercentage), currentScale.y, currentScale.z);

        playerLaunchMeter.localScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * updateSpeed);

        Color lerpedColor = Color.Lerp(Color.red, Color.cyan, fillPercentage);
        launchMeterRenderer.material.color = lerpedColor;
    }

    public void UpdatePlayerHealCharge()
    {
        for (int i = 0; i < playerHealCharge.Length; i++)
        {
            Renderer healChargeRenderer = playerHealCharge[i].GetComponent<Renderer>();

            if (i < Player.currentHealCharge)
            {
                healChargeRenderer.material.color = Color.cyan;
            }
            else
            {
                healChargeRenderer.material.color = Color.gray;
            }
        }
    }

    public void UpdatePlayerAmmoCount()
    {
        if (Player.currentAmmo <= 0) playerAmmoCount.color = Color.red;
        else playerAmmoCount.color = Color.white;

        playerAmmoCount.text = Player.currentAmmo.ToString();
    }

    public void UpdateRightCrosshair(string animName)
    {
        rightCrosshairAnimator.SetTrigger(animName);
    }

    public void UpdateRightCrosshairEnemy(bool onEnemy)
    {
        rightCrosshairAnimator.SetBool("On Enemy", onEnemy);
    }

    public void UpdateLeftCrosshair(string animName)
    {
        leftCrosshairAnimator.SetTrigger(animName);
    }

    public void UpdateRightCrosshairDetection()
    {
        Ray ray = MainCamera.ScreenPointToRay(rightCrosshair.transform.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, crosshairDetectionRange, enemyLayer))
        {
            if (!rightCrosshairOnEnemy)
            {
                rightCrosshairOnEnemy = true;
                UpdateRightCrosshairEnemy(true);
            }
        }
        else
        {
            if (rightCrosshairOnEnemy)
            {
                rightCrosshairOnEnemy = false;
                UpdateRightCrosshairEnemy(false);
            }
        }
    }

    public void ToggleLeftCrosshair(bool inIronmanState)
    {
        if (inIronmanState && leftCrosshair.activeSelf) leftCrosshair.SetActive(false); 
        else if (!inIronmanState && !leftCrosshair.activeSelf) leftCrosshair.SetActive(true);

        if (inIronmanState)
        {
            if (!leftCrosshair.activeSelf) return; //Hide left crosshair if in ironman
            leftCrosshair.SetActive(false);
        }
        else if (!inIronmanState)
        {
            if (leftCrosshair.activeSelf) return;
            leftCrosshair.SetActive(true);
        }
    }
    #endregion

    #region Enemy Indicator UI
    private void UpdateEnemyDetection()
    {
        // Reuse array to reduce garbage collection
        Collider[] enemiesInRange = new Collider[20]; // Adjust size based on your needs
        int numEnemies = Physics.OverlapSphereNonAlloc(Player.transform.position, enemyDetectionRadius, enemiesInRange, enemyLayer);

        // Use HashSet for faster lookups
        HashSet<GameObject> currentEnemies = new HashSet<GameObject>();

        // Process found enemies
        for (int i = 0; i < numEnemies; i++)
        {
            GameObject enemy = enemiesInRange[i].gameObject;
            currentEnemies.Add(enemy);

            if (!enemyIndicators.ContainsKey(enemy))
            {
                GameObject indicator = Instantiate(enemyIndicatorPrefab, indicatorParent);
                enemyIndicators.Add(enemy, indicator);
            }
        }

        // Remove old indicators
        List<GameObject> enemiesToRemove = new List<GameObject>();
        foreach (var enemy in enemyIndicators.Keys)
        {
            if (!currentEnemies.Contains(enemy))
            {
                enemiesToRemove.Add(enemy);
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            Destroy(enemyIndicators[enemy]);
            enemyIndicators.Remove(enemy);
        }
    }

    private void UpdateIndicatorPositions()
    {
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        var mainCamTransform = MainCamera.transform;

        foreach (var kvp in enemyIndicators)
        {
            GameObject enemy = kvp.Key;
            GameObject indicator = kvp.Value;
            Vector3 directionToEnemy = enemy.transform.position - mainCamTransform.position;

            // Early out if enemy is too far
            float distanceToEnemy = directionToEnemy.magnitude;
            if (distanceToEnemy > enemyDetectionRadius)
                continue;

            Vector3 viewportPoint = MainCamera.WorldToViewportPoint(enemy.transform.position);
            bool isInFront = viewportPoint.z > 0;
            bool isInViewport = isInFront &&
                               viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                               viewportPoint.y >= 0 && viewportPoint.y <= 1;

            // Cache the indicator's Image component
            var indicatorImage = indicator.GetComponent<Image>();

            if (isInViewport)
            {
                indicatorImage.color = new Color(1, 1, 1, 0f);
                continue;
            }

            indicatorImage.color = Color.white;

            // Calculate screen position and direction
            Vector3 enemyScreenPos = MainCamera.WorldToScreenPoint(enemy.transform.position);
            Vector2 directionOnScreen = ((Vector2)enemyScreenPos - (Vector2)screenCenter).normalized;

            // Calculate edge position and set indicator position
            Vector2 indicatorPos = CalculateEdgePosition(directionOnScreen);
            indicator.transform.SetPositionAndRotation(
                indicatorPos,
                Quaternion.Euler(0, 0, DetermineEdgeRotation(indicatorPos))
            );

            // Update scale
            float scale = Mathf.Lerp(2f, 0.5f, distanceToEnemy / enemyDetectionRadius);
            indicator.transform.localScale = new Vector3(scale, scale, 1);
        }
    }

    private Vector2 CalculateEdgePosition(Vector2 direction)
    {
        const float offset = 10f;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float halfScreenWidth = screenWidth * 0.5f;
        float halfScreenHeight = screenHeight * 0.5f;

        bool isHorizontalEdge = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
        float slope = direction.y / direction.x;

        if (isHorizontalEdge)
        {
            float edgeX = direction.x > 0 ? screenWidth - offset : offset;
            float edgeY = halfScreenHeight + (slope * (edgeX - halfScreenWidth));
            return new Vector2(edgeX, Mathf.Clamp(edgeY, offset, screenHeight - offset));
        }
        else
        {
            float edgeY = direction.y > 0 ? screenHeight - offset : offset;
            float edgeX = halfScreenWidth + ((edgeY - halfScreenHeight) / slope);
            return new Vector2(Mathf.Clamp(edgeX, offset, screenWidth - offset), edgeY);
        }
    }

    private float DetermineEdgeRotation(Vector2 position)
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        const float edgeThreshold = 20f;

        // Check edges in order of likelihood
        if (position.x >= screenWidth - edgeThreshold) return RIGHT_ROTATION;
        if (position.x <= edgeThreshold) return LEFT_ROTATION;
        if (position.y >= screenHeight - edgeThreshold) return TOP_ROTATION;
        if (position.y <= edgeThreshold) return BOTTOM_ROTATION;

        // If not on an edge, find closest edge
        float distToRight = screenWidth - position.x;
        float distToLeft = position.x;
        float distToTop = screenHeight - position.y;
        float distToBottom = position.y;

        float minDist = Mathf.Min(distToRight, distToLeft, distToTop, distToBottom);

        if (minDist == distToRight) return RIGHT_ROTATION;
        if (minDist == distToLeft) return LEFT_ROTATION;
        if (minDist == distToTop) return TOP_ROTATION;
        return BOTTOM_ROTATION;
    }
    #endregion

    #region Other UI
    private IEnumerator FadeToggle(bool fadeIn)
    {
        fadePrefab.SetActive(true);

        if (fadeIn)
        {
            fadePrefab.GetComponent<Animator>().SetTrigger("Fade In");
            yield return new WaitForSeconds(1f);
            fadePrefab.SetActive(false);
        }
        else
        {
            fadePrefab.GetComponent<Animator>().SetTrigger("Fade Out");
            yield return new WaitForSeconds(1f);
            StartCoroutine(GameManager.ReloadScene());
        }
    }
    
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

    #region Pause UI
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

    public void ResumeGame()
    {
        GameManager.ChangeGameState(GameManager.GameState.PLAY);
        ShowPauseMenu(false); 
    }

    public void RestartGame()
    {
        GameManager.ChangeGameState(GameManager.GameState.PLAY);
        ShowPauseMenu(false);
        StartCoroutine(FadeToggle(false)); //Fade out
    }

    public void GoToSettings()
    {

    }

    public void ReturnToMainMenu()
    {

    }
    #endregion
}
