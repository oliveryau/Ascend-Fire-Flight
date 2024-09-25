using UnityEngine;

public class EnemyTriggerZone : MonoBehaviour
{
    [Header("Enemy Variables")]
    public EnemyController[] EnemyList;
    public Transform[] EnemySpawnPoints;
    public int enemySpawned;
    public int enemyDead;

    [Header("Gate Variables")]
    public GameObject gate;

    private bool isTriggered;
    [HideInInspector] public bool endSegment;

    private void Start()
    {
        enemySpawned = EnemyList.Length;
        enemyDead = 0;
    }

    private void Update()
    {
        if (endSegment) return;
        if (!isTriggered) return;

        CloseGate();

        if (enemyDead >= enemySpawned) OpenGate();
    }

    private void SpawnEnemies()
    {
        int spawnCount = Mathf.Min(EnemyList.Length, EnemySpawnPoints.Length);

        for (int i = 0; i < spawnCount; i++)
        {
            Instantiate(EnemyList[i], EnemySpawnPoints[i].position, EnemySpawnPoints[i].rotation);
            EnemyList[i].TriggerZone = this;
        }

        isTriggered = true;
    }

    private void CloseGate()
    {
        gate.GetComponent<Gate>().closedGate = true;
    }

    private void OpenGate()
    {
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
