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
    public GameObject mainGameUi;
    [HideInInspector] public float updateSpeed = 5f;

    [Header("Player HP UI")]
    public Image playerHealthFill;

    private float targetHealthFill;

    [Header("Player Launch UI")]
    public Image playerLaunchFill;

    private float targetLaunchFill;

    [Header("Player Ammo UI")]
    public TextMeshProUGUI playerAmmoText;

    [Header("Player Crosshair UI")]
    public GameObject rightCrosshair;
    public GameObject leftCrosshair;
    public float crosshairDetectionRange;

    private Animator rightCrosshairAnimator;
    private Animator leftCrosshairAnimator;
    private bool rightCrosshairOnEnemy;
    private bool crosshairOnSpawner;

    [Header("Other Player UI")]
    public GameObject playerHealUi;
    public GameObject tutorialCue;

    [Header("Enemy Indicator UI")]
    public GameObject enemyIndicatorPrefab;
    public GameObject enemySpawnerIndicatorPrefab;
    public float detectionRadius;
    public float indicatorDistance; //Distance from screen center (0-1)
    public Transform indicatorParent;
    public LayerMask enemyLayer;
    public LayerMask spawnerLayer;

    private Dictionary<GameObject, GameObject> enemyIndicators = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, GameObject> spawnerIndicators = new Dictionary<GameObject, GameObject>();
    private const float RIGHT_ROTATION = -90f;
    private const float LEFT_ROTATION = 90f;
    private const float TOP_ROTATION = 180f;
    private const float BOTTOM_ROTATION = 0f;

    [Header("Enemy HP UI")]
    public Image enemyBossHealthFill;
    public Image enemySpawnerHealthFill;

    [SerializeField] private Color originalColor;
    [SerializeField] private Color flashColor;
    private float flashDuration = 0.1f;
    private bool isFlashing = false;
    private float flashTimer = 0f;
    private float targetEnemyBossHealthFill;
    private float targetEnemySpawnerHealthFill;
    private EnemyBossMeleeSpawner currentTargetedSpawner;

    [Header("Non-Player UI")]
    public GameObject fadePrefab;
    public Image playerFallingOutOfBoundsOverlay;
    public Image playerDamagedOverlay;

    [Header("Adaptive UI")]
    public GameObject sprintUi;
    public GameObject launchUi;
    public GameObject rightWeaponUi;
    public GameObject leftWeaponUi;
    public GameObject ammoUi;
    public GameObject crosshairUi;
    public GameObject rightDecorativeUi;
    public GameObject reloadUi;
    public GameObject enemyBossHealthUi;
    public GameObject enemyBossSpawnerHealthUi;
    public GameObject[] leftOverlayUi;

    [Header("References")]
    private GameManager GameManager;
    private PlayerController Player;
    private Camera MainCamera;
    private TutorialManager TutorialManager;
    #endregion

    #region Initialization
    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();
        Player = FindFirstObjectByType<PlayerController>();
        MainCamera = Camera.main;

        InitializeUi();
    }

    private void Update()
    {
        if (GameManager.currentGameState != GameManager.GameState.PAUSE || Player.currentPlayerState != PlayerController.PlayerState.DEAD)
        {
            ToggleUi(true);
            UpdatePlayerHealthBar();
            UpdatePlayerLaunchMeter();
            UpdateRightCrosshairDetection();
            UpdateSpawnerHealthDetection();
            UpdateEnemyDetection();
            UpdateIndicatorPositions();
        }

        if (GameManager.currentGameState == GameManager.GameState.PAUSE)
        {
            ToggleUi(false);
        }
    }

    private void InitializeUi()
    {
        StartCoroutine(GameManager.FadeToggle(true)); //Fade in

        targetHealthFill = Player.currentHealth / Player.maxHealth;
        playerHealthFill.fillAmount = targetHealthFill;

        targetLaunchFill = Player.currentLaunchMeter / Player.maxLaunchMeter;
        playerLaunchFill.fillAmount = targetLaunchFill;

        UpdatePlayerAmmoCount();

        rightCrosshairAnimator = rightCrosshair.GetComponent<Animator>();
        leftCrosshairAnimator = leftCrosshair.GetComponent<Animator>();
    }

    private void ToggleUi(bool active)
    {
        mainGameUi.SetActive(active);
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
        targetLaunchFill = Player.currentLaunchMeter / Player.maxLaunchMeter;
        playerLaunchFill.fillAmount = Mathf.Lerp(playerLaunchFill.fillAmount, targetLaunchFill, Time.deltaTime * updateSpeed);

        Color lerpedColor = Color.Lerp(Color.black, Color.white, playerLaunchFill.fillAmount);
        playerLaunchFill.color = lerpedColor;
    }

    public void UpdatePlayerAmmoCount()
    {
        if (Player.currentAmmo <= 0.2f * Player.maxAmmo) playerAmmoText.color = Color.red;
        else playerAmmoText.color = Color.white;

        playerAmmoText.text = Player.currentAmmo.ToString() + " / " + Player.maxAmmo.ToString();
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

    public void ToggleHealCue(bool canHeal)
    {
        playerHealUi.SetActive(canHeal);
    }

    public void OverlayLeftUi(bool showOverlay)
    {
        foreach (var overlay in leftOverlayUi)
        {
            overlay.SetActive(showOverlay);
        }
    }
    #endregion

    #region Enemy Indicator UI
    private void UpdateEnemyDetection()
    {
        // Reuse array to reduce garbage collection
        Collider[] enemiesInRange = new Collider[20]; // Adjust size based on your needs
        int numEnemies = Physics.OverlapSphereNonAlloc(Player.transform.position, detectionRadius, enemiesInRange, enemyLayer);

        // Use HashSet for faster lookups
        HashSet<GameObject> currentEnemies = new HashSet<GameObject>();

        // Process found enemies
        for (int i = 0; i < numEnemies; i++)
        {
            if (enemiesInRange[i] == null) continue;

            GameObject enemy = enemiesInRange[i].gameObject;
            currentEnemies.Add(enemy);

            if (!enemyIndicators.ContainsKey(enemy))
            {
                GameObject indicator = Instantiate(enemyIndicatorPrefab, indicatorParent);
                enemyIndicators.Add(enemy, indicator);
            }
        }

        Collider[] spawnersInRange = new Collider[10];
        int numSpawners = Physics.OverlapSphereNonAlloc(Player.transform.position, detectionRadius, spawnersInRange, spawnerLayer);

        HashSet<GameObject> currentSpawners = new HashSet<GameObject>();

        // Process found spawners
        for (int i = 0; i < numSpawners; i++)
        {
            if (spawnersInRange[i] == null) continue;

            GameObject spawner = spawnersInRange[i].gameObject;
            currentSpawners.Add(spawner);

            if (!spawnerIndicators.ContainsKey(spawner))
            {
                GameObject indicator = Instantiate(enemySpawnerIndicatorPrefab, indicatorParent);
                spawnerIndicators.Add(spawner, indicator);
            }
        }

        // Clean up enemy indicators
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

        // Clean up spawner indicators
        List<GameObject> spawnersToRemove = new List<GameObject>();
        foreach (var spawner in spawnerIndicators.Keys)
        {
            if (!currentSpawners.Contains(spawner))
            {
                spawnersToRemove.Add(spawner);
            }
        }

        foreach (var spawner in spawnersToRemove)
        {
            Destroy(spawnerIndicators[spawner]);
            spawnerIndicators.Remove(spawner);
        }
    }

    private void UpdateIndicatorPositions()
    {
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        var mainCamTransform = MainCamera.transform;

        // Update enemy indicators
        UpdateIndicatorGroup(enemyIndicators, detectionRadius, screenCenter, mainCamTransform);

        // Update spawner indicators
        UpdateIndicatorGroup(spawnerIndicators, detectionRadius, screenCenter, mainCamTransform);
    }

    private void UpdateIndicatorGroup(Dictionary<GameObject, GameObject> indicators, float detectionRadius, Vector3 screenCenter, Transform mainCamTransform)
    {
        foreach (var kvp in indicators)
        {
            GameObject target = kvp.Key;
            GameObject indicator = kvp.Value;
            Vector3 directionToTarget = target.transform.position - mainCamTransform.position;

            // If too far from enemy/spawner
            float distanceToTarget = directionToTarget.magnitude;
            if (distanceToTarget > detectionRadius)
                continue;

            Vector3 viewportPoint = MainCamera.WorldToViewportPoint(target.transform.position);
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
            Vector3 enemyScreenPos = MainCamera.WorldToScreenPoint(target.transform.position);
            Vector2 directionOnScreen = ((Vector2)enemyScreenPos - (Vector2)screenCenter).normalized;

            // Calculate edge position and set indicator position
            Vector2 indicatorPos = CalculateEdgePosition(directionOnScreen);
            indicator.transform.SetPositionAndRotation(
                indicatorPos,
                Quaternion.Euler(0, 0, DetermineEdgeRotation(indicatorPos))
            );

            // Update scale based on distance
            float scale = Mathf.Lerp(2f, 0.5f, distanceToTarget / detectionRadius);
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

    #region Enemy Boss UI
    public void UpdateBossEnemyHealthBar(EnemyBoss boss)
    {
        targetEnemyBossHealthFill = boss.currentHealth / boss.maxHealth;
        enemyBossHealthFill.fillAmount = Mathf.Lerp(enemyBossHealthFill.fillAmount, targetEnemyBossHealthFill, Time.deltaTime * updateSpeed);
    }

    public void UpdateBossEnemyHealthFlash(EnemyBoss boss)
    {
        if (!isFlashing) return;

        flashTimer += Time.deltaTime;
        if (flashTimer >= flashDuration)
        {
            enemyBossHealthFill.color = originalColor;
            isFlashing = false;
            flashTimer = 0f;
        }
    }

    public void FlashBossEnemyHealthBar()
    {
        enemyBossHealthFill.color = flashColor;
        isFlashing = true;
        flashTimer = 0f;
    }

    private void UpdateSpawnerHealthDetection()
    {
        Ray ray = MainCamera.ScreenPointToRay(crosshairUi.transform.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, crosshairDetectionRange, spawnerLayer))
        {
            var spawner = hit.collider.GetComponent<EnemyBossMeleeSpawner>();
            if (spawner != null)
            {
                if (!crosshairOnSpawner || currentTargetedSpawner != spawner) //Only update if looking at different spawner or none previously
                {
                    crosshairOnSpawner = true;
                    currentTargetedSpawner = spawner;
                    DisplaySpawnerHealth(true);
                    UpdateEnemySpawnerHealthBar(spawner);
                }
                else if (currentTargetedSpawner == spawner) //Continue updating while looking at the same spawner
                {
                    UpdateEnemySpawnerHealthBar(spawner);
                }
            }
        }
        else
        {
            if (crosshairOnSpawner)
            {
                crosshairOnSpawner = false;
                currentTargetedSpawner = null;
                DisplaySpawnerHealth(false);
            }
        }
    }

    private void DisplaySpawnerHealth(bool onSpawner)
    {
        enemyBossSpawnerHealthUi.SetActive(onSpawner);
    }

    public void UpdateEnemySpawnerHealthBar(EnemyBossMeleeSpawner spawner)
    {
        if (spawner == null) return;

        targetEnemySpawnerHealthFill = spawner.currentHealth / spawner.maxHealth;
        enemySpawnerHealthFill.fillAmount = Mathf.Lerp(enemySpawnerHealthFill.fillAmount, targetEnemySpawnerHealthFill, Time.deltaTime * updateSpeed);
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
        StartCoroutine(GameManager.FadeToggle(false, "Main Scene")); //Fade out, same scene
    }

    public void GoToSettings()
    {

    }

    public void ReturnToMainMenu()
    {
        StartCoroutine(GameManager.FadeToggle(false, "Main Menu")); //Fade out, main menu
    }
    #endregion
}
