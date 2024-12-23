using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBossMeleeSpawner : MonoBehaviour
{
    [Header("Spawner Variables")]
    public float maxHealth;
    public float currentHealth;
    public float spawnInterval;
    public GameObject mainParticle;

    private Animator Animator;
    private bool isActivated;
    private bool isSpawning;

    [Header("Enemy Variables")]
    public EnemyController EnemyToSpawn;
    public Transform[] EnemySpawnPoints;
    public List<GameObject> CurrentEnemiesAlive;
    public int maxEnemies;

    private int currentSpawnPoint;
    private UiManager UiManager;

    private void Start()
    {
        UiManager = FindFirstObjectByType<UiManager>();

        currentHealth = maxHealth;

        Animator = GetComponent<Animator>();

        CurrentEnemiesAlive = new List<GameObject>();
        CurrentEnemiesAlive.Clear();

        currentSpawnPoint = 0;
    }

    private void Update()
    {
        if (currentHealth <= 0 || !isActivated) return;

        if (!mainParticle.activeSelf) mainParticle.SetActive(true);

        CurrentEnemiesAlive.RemoveAll(enemy => enemy == null);
        if (!isSpawning && CurrentEnemiesAlive.Count < maxEnemies)
        {
            StartCoroutine(SpawnEnemyWithDelay());
        }
    }

    public void SpawnMeleeEnemies()
    {
        if (isSpawning) return;

        isActivated = true;
        //AudioManager.Instance.PlayOneShot("Enemy Boss Spawn", gameObject);
        if (gameObject.layer != LayerMask.NameToLayer("Spawner"))
        {
            gameObject.layer = LayerMask.NameToLayer("Spawner"); //For indicators
        }
    }

    private IEnumerator SpawnEnemyWithDelay()
    {
        isSpawning = true;
        yield return new WaitForSeconds(spawnInterval);

        if (CurrentEnemiesAlive.Count < maxEnemies)
        {
            SpawnSingleEnemy();
        }

        isSpawning = false;
    }

    private void SpawnSingleEnemy()
    {
        Animator.SetTrigger("Spawning");
        currentSpawnPoint = (currentSpawnPoint + 1) % EnemySpawnPoints.Length;
        EnemyController newEnemy = Instantiate(EnemyToSpawn, EnemySpawnPoints[currentSpawnPoint].position, EnemySpawnPoints[currentSpawnPoint].rotation);
        CurrentEnemiesAlive.Add(newEnemy.gameObject);
    }

    public void TakeDamage(float damageTaken)
    {
        if (currentHealth <= 0 || !isActivated) return;

        currentHealth -= damageTaken;
        Animator.SetTrigger("Damaged");
        UiManager.enemyBossSpawnerHealthUi.GetComponent<Animator>().SetTrigger("Fire");

        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(SpawnerDestroy());

            if (gameObject.layer != LayerMask.NameToLayer("Default"))
            {
                gameObject.layer = LayerMask.NameToLayer("Default"); //For indicators
            }
        }
    }

    private IEnumerator SpawnerDestroy()
    {
        isActivated = false;
        foreach (var enemy in CurrentEnemiesAlive)
        {
            enemy.GetComponent<EnemyController>().currentEnemyState = EnemyController.EnemyState.DEAD;
        }
        Animator.SetTrigger("Dead");
        AudioManager.Instance.PlayOneShot("Enemy Spawner Death", gameObject);
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
}
