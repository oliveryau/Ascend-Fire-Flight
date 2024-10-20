using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBossMeleeSpawner : MonoBehaviour
{
    [Header("Spawner Variables")]
    public float maxHealth;
    public float currentHealth;
    public float spawnInterval;

    private Animator Animator;
    private bool isActivated;
    private bool isSpawning;
    private bool initialSpawnComplete;

    [Header("Enemy Variables")]
    public EnemyController EnemyToSpawn;
    public Transform[] EnemySpawnPoints;
    public List<GameObject> CurrentEnemiesAlive;
    public int maxEnemies;

    private int currentSpawnPoint;

    private void Start()
    {
        currentHealth = maxHealth;

        Animator = GetComponent<Animator>();

        CurrentEnemiesAlive = new List<GameObject>();
        CurrentEnemiesAlive.Clear();

        currentSpawnPoint = 0;
    }

    private void Update()
    {
        if (currentHealth <= 0 || !isActivated) return;

        CurrentEnemiesAlive.RemoveAll(enemy => enemy == null);
        if (!isSpawning && CurrentEnemiesAlive.Count < maxEnemies && initialSpawnComplete)
        {
            StartCoroutine(SpawnSingleWithDelay());
        }
    }

    public void SpawnMeleeEnemies()
    {
        if (isSpawning) return;

        InitialSpawnSequence();
    }

    private void InitialSpawnSequence()
    {
        isActivated = true;
        isSpawning = true;

        for (int i = 0; i < EnemySpawnPoints.Length && CurrentEnemiesAlive.Count < maxEnemies; i++)
        {
            SpawnSingleEnemy();
        }

        initialSpawnComplete = true;
        isSpawning = false;
    }

    private IEnumerator SpawnSingleWithDelay()
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
        currentSpawnPoint = (currentSpawnPoint + 1) % EnemySpawnPoints.Length;
        EnemyController newEnemy = Instantiate(EnemyToSpawn, EnemySpawnPoints[currentSpawnPoint].position, EnemySpawnPoints[currentSpawnPoint].rotation);
        CurrentEnemiesAlive.Add(newEnemy.gameObject);
    }

    public void TakeDamage(float damageTaken)
    {
        if (currentHealth <= 0 || !isActivated) return;

        currentHealth -= damageTaken;
        //Animator.SetTrigger("Damaged");

        if (currentHealth <= 0)
        {
            StartCoroutine(SpawnerDestroy());
        }
    }

    private IEnumerator SpawnerDestroy()
    {
        isActivated = false;
        foreach (var enemy in CurrentEnemiesAlive)
        {
            enemy.GetComponent<EnemyController>().currentEnemyState = EnemyController.EnemyState.DEAD;
        }
        //Animator.SetTrigger("Dead");
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
