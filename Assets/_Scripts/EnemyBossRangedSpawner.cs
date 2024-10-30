using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBossRangedSpawner : MonoBehaviour
{
    [Header("Spawner Variables")]
    public float spawnInterval;
    public bool isActivated;

    private bool isSpawning;

    [Header("Enemy Variables")]
    public EnemyController EnemyToSpawn;
    public Transform[] EnemySpawnPoints;
    public List<GameObject> CurrentEnemiesAlive;
    public int maxEnemies;

    private int currentSpawnPoint;

    private void Start()
    {
        CurrentEnemiesAlive = new List<GameObject>();
        CurrentEnemiesAlive.Clear();

        currentSpawnPoint = 0;
    }

    private void Update()
    {
        if (!isActivated) return;

        CurrentEnemiesAlive.RemoveAll(enemy => enemy == null);
        if (!isSpawning && CurrentEnemiesAlive.Count < maxEnemies)
        {
            StartCoroutine(SpawnEnemyWithDelay());
        }
    }

    public void SpawnRangedEnemies()
    {
        if (isSpawning) return;

        isActivated = true;
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
        currentSpawnPoint = (currentSpawnPoint + 1) % EnemySpawnPoints.Length;
        EnemyController newEnemy = Instantiate(EnemyToSpawn, EnemySpawnPoints[currentSpawnPoint].position, EnemySpawnPoints[currentSpawnPoint].rotation);
        CurrentEnemiesAlive.Add(newEnemy.gameObject);
    }

    public void SpawnerStop()
    {
        StopAllCoroutines();
        foreach (var enemy in CurrentEnemiesAlive)
        {
            enemy.GetComponent<EnemyController>().currentEnemyState = EnemyController.EnemyState.DEAD;
        }

        isActivated = false;
    }
}
