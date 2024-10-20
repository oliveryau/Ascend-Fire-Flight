using System.Collections.Generic;
using UnityEngine;

public class EnemyTriggerZone : MonoBehaviour
{
    [Header("Enemy Variables")]
    public EnemyController[] EnemyToSpawn;
    public Transform[] EnemySpawnPoints;
    public List<GameObject> CurrentEnemiesAlive;

    private int totalEnemies;
    private int totalEnemiesDead;

    [Header("Gate Variables")]
    public GameObject gate;

    private bool isTriggered;
    private bool endSegment;

    private void Start()
    {
        CurrentEnemiesAlive = new List<GameObject>();
        CurrentEnemiesAlive.Clear();

        totalEnemies = EnemyToSpawn.Length;
        totalEnemiesDead = 0;
    }

    private void Update()
    {
        if (endSegment) return;
        if (!isTriggered) return;

        CloseGate();
        CheckDeadEnemies();

        if (totalEnemiesDead >= totalEnemies) OpenGate();
    }

    private void SpawnEnemies()
    {
        int spawnCount = Mathf.Min(EnemyToSpawn.Length, EnemySpawnPoints.Length);

        for (int i = 0; i < spawnCount; i++)
        {
            EnemyController newEnemy = Instantiate(EnemyToSpawn[i], EnemySpawnPoints[i].position, EnemySpawnPoints[i].rotation);
            CurrentEnemiesAlive.Add(newEnemy.gameObject);
        }

        isTriggered = true;
    }

    private void CheckDeadEnemies()
    {
        for (int i = CurrentEnemiesAlive.Count - 1; i >= 0; i--)
        {
            if (CurrentEnemiesAlive[i] == null)
            {
                ++totalEnemiesDead;
                CurrentEnemiesAlive.RemoveAt(i);
            }
        }
    }

    private void CloseGate()
    {
        if (gate == null) return;

        gate.GetComponent<Gate>().closedGate = true;
    }

    private void OpenGate()
    {
        if (gate == null) return;

        gate.GetComponent<Gate>().closedGate = false;
        endSegment = true;
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player") && !isTriggered)
        {
            CloseGate();
            SpawnEnemies();
        }
    }
}
