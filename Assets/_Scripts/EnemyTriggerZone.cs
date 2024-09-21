using UnityEngine;

public class EnemyTriggerZone : MonoBehaviour
{
    public EnemyController[] EnemyList;
    public Transform[] EnemySpawnPoints;

    private bool isTriggered;

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player") && !isTriggered)
        {
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        int spawnCount = Mathf.Min(EnemyList.Length, EnemySpawnPoints.Length);

        for (int i = 0; i < spawnCount; i++)
        {
            Instantiate(EnemyList[i], EnemySpawnPoints[i].position, EnemySpawnPoints[i].rotation);
        }

        isTriggered = true;
    }
}
