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

    private float targetHealthFill;

    [Header("Diegetic UI")]
    public Transform playerLaunchMeter;
    public GameObject[] playerHealCharge;
    public TextMeshProUGUI playerAmmoCount;

    private Renderer launchMeterRenderer;

    [Header("Enemy Indicator UI")]
    public GameObject enemyIndicatorPrefab;
    public float detectionRadius;
    public float indicatorDistance; //Distance from screen center (0-1)
    public LayerMask enemyLayer;
    public Transform indicatorParent;

    private Dictionary<GameObject, GameObject> enemyIndicators = new Dictionary<GameObject, GameObject>();

    [Header("Other UI")]
    public Image playerFallingOutOfBoundsOverlay;
    public Image playerDamagedOverlay;

    [Header("References")]
    private GameManager GameManager;
    private PlayerController Player;
    private Camera MainCamera;
    #endregion

    private void Start()
    {
        GameManager = FindFirstObjectByType<GameManager>();
        Player = FindFirstObjectByType<PlayerController>();
        MainCamera = Camera.main;

        InitializeUiValues();
    }

    private void Update()
    {
        UpdatePlayerHealthBar();
        UpdatePlayerLaunchMeter();
        UpdatePlayerHealCharge();
        UpdateEnemyDetection();
        UpdateIndicatorPositions();
    }

    private void InitializeUiValues()
    {
        targetHealthFill = Player.currentHealth / Player.maxHealth;
        playerHealthFill.fillAmount = targetHealthFill;

        launchMeterRenderer = playerLaunchMeter.GetComponent<Renderer>();

        UpdatePlayerAmmoCount();
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

        Color lerpedColor = Color.Lerp(Color.gray, Color.green, fillPercentage);
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
        if (Player.currentAmmo <= 0) playerAmmoCount.color = Color.red;
        else playerAmmoCount.color = Color.white;

        playerAmmoCount.text = Player.currentAmmo.ToString();
    }
    #endregion

    #region Enemy Indicator UI
    private void UpdateEnemyDetection()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(Player.transform.position, detectionRadius, enemyLayer);

        // Remove indicators for enemies no longer in range
        List<GameObject> enemiesToRemove = new List<GameObject>();
        foreach (var enemy in enemyIndicators.Keys)
        {
            //if (enemy.GetComponent<EnemyController>().currentEnemyState == EnemyController.EnemyState.DEAD)
            if (!System.Array.Exists(enemiesInRange, e => e.gameObject == enemy))
            {
                enemiesToRemove.Add(enemy);
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            Destroy(enemyIndicators[enemy]);
            enemyIndicators.Remove(enemy);
        }

        // Add indicators for new enemies in range
        foreach (var enemyCollider in enemiesInRange)
        {
            if (!enemyIndicators.ContainsKey(enemyCollider.gameObject))
            {
                GameObject indicator = Instantiate(enemyIndicatorPrefab, indicatorParent);
                enemyIndicators.Add(enemyCollider.gameObject, indicator);
            }
        }
    }

    private void UpdateIndicatorPositions()
    {
        foreach (var kvp in enemyIndicators)
        {
            GameObject enemy = kvp.Key;
            GameObject indicator = kvp.Value;
            Vector3 directionToEnemy = enemy.transform.position - MainCamera.transform.position;
            float distanceToEnemy = directionToEnemy.magnitude;

            //Convert world position to viewport position
            Vector3 viewportPoint = MainCamera.WorldToViewportPoint(enemy.transform.position);

            //Calculate the angle to the enemy in relation to the camera's forward direction
            Vector3 enemyScreenPos = MainCamera.WorldToScreenPoint(enemy.transform.position);
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Vector2 directionOnScreen = (enemyScreenPos - screenCenter).normalized;

            //Calculate the position on the screen edge
            Vector2 indicatorPos = CalculateEdgePosition(directionOnScreen);

            //Set the indicator position
            indicator.transform.position = indicatorPos;

            //Determine the rotation based on which edge the indicator is on
            float rotation = DetermineEdgeRotation(indicatorPos);
            indicator.transform.rotation = Quaternion.Euler(0, 0, rotation);

            //Determine if the enemy is in front of the camera
            bool isInFront = viewportPoint.z > 0;

            //Check if enemy is within the viewport bounds
            bool isInViewport = viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                                viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                                isInFront;

            if (isInViewport)
            {
                //Enemy is in viewport, make the indicator transparent
                indicator.GetComponent<Image>().color = new Color(255, 255, 255, 0f);
            }
            else
            {
                //Enemy is out of viewport, make the indicator fully opaque
                indicator.GetComponent<Image>().color = Color.white;
            }

            //Adjust the indicator's size based on distance
            float scale = Mathf.Lerp(2f, 0.5f, distanceToEnemy / detectionRadius);
            indicator.transform.localScale = new Vector3(scale, scale, 1);
        }
    }

    private Vector2 CalculateEdgePosition(Vector2 direction)
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float offset = 10f; // Distance from the screen edge

        float slope = direction.y / direction.x;
        float edgeX, edgeY;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            //Indicator will be on the left or right edge
            edgeX = (direction.x > 0) ? screenWidth - offset : offset;
            edgeY = (screenHeight / 2) + (slope * (edgeX - (screenWidth / 2)));
            edgeY = Mathf.Clamp(edgeY, offset, screenHeight - offset);
        }
        else
        {
            //Indicator will be on the top or bottom edge
            edgeY = (direction.y > 0) ? screenHeight - offset : offset;
            edgeX = (screenWidth / 2) + ((edgeY - (screenHeight / 2)) / slope);
            edgeX = Mathf.Clamp(edgeX, offset, screenWidth - offset);
        }

        return new Vector2(edgeX, edgeY);
    }

    private float DetermineEdgeRotation(Vector2 position)
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float edgeThreshold = 20f; //Threshold to determine if we're on an edge

        if (Mathf.Abs(position.x - screenWidth) <= edgeThreshold)
            return -90f; //Right edge, point left
        else if (position.x <= edgeThreshold)
            return 90f; //Left edge, point right
        else if (Mathf.Abs(position.y - screenHeight) <= edgeThreshold)
            return 180f; //Top edge, point down
        else if (position.y <= edgeThreshold)
            return 0f; //Bottom edge, point up

        // If not on an edge, determine the closest edge
        float distToRight = screenWidth - position.x;
        float distToLeft = position.x;
        float distToTop = screenHeight - position.y;
        float distToBottom = position.y;

        float minDist = Mathf.Min(distToRight, distToLeft, distToTop, distToBottom);

        if (minDist == distToRight)
            return -90f; //Closest to right edge, point left
        else if (minDist == distToLeft)
            return 90f; //Closest to left edge, point right
        else if (minDist == distToTop)
            return 180f; //Closest to top edge, point down
        else
            return 0f; //Closest to bottom edge, point up
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
