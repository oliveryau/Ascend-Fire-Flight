using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { WAIT, PATROL, ALERT, ATTACK }
    public EnemyState currentEnemyState;

    [Header("Enemy Base Variables")]
    public float maxHealth;
    public float currentHealth;

    [Header("Enemy Patrol Points")]
    public Transform[] patrolPoints;
    private int currentPatrolPointIndex;

    [Header("Enemy Attack Variables")]
    public float attackDamage;
    public float attackCooldown;
    private float lastAttackTime;

    [Header("References")]
    public SphereCollider DetectionRadius;
    public SphereCollider AttackRadius;
    private PlayerController Player;
    private NavMeshAgent NavMeshAgent;

    private void Start()
    {
        currentEnemyState = EnemyState.ALERT;

        Player = FindFirstObjectByType<PlayerController>();
        NavMeshAgent = GetComponent<NavMeshAgent>();

        NavMeshAgent.SetDestination(patrolPoints[0].position);

        lastAttackTime = -attackCooldown;
    }

    private void Update()
    {
        CheckEnemyState();
    }

    private void CheckEnemyState()
    {
        switch (currentEnemyState)
        {
            case EnemyState.WAIT:
                break;
            case EnemyState.PATROL:
                Patrol();
                break;
            case EnemyState.ALERT:
                Alert(); //Move towards player
                break;
            case EnemyState.ATTACK:
                Attack();
                break;
        }
    }

    #region Patrol
    private void Patrol()
    {
        NavMeshAgent.isStopped = false;

        if (NavMeshAgent.remainingDistance < 0.1f)
        {
            currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
            NavMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
            transform.LookAt(patrolPoints[currentPatrolPointIndex].position);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer <= DetectionRadius.radius)
        {
            currentEnemyState = EnemyState.ALERT;
        }
    }
    #endregion

    #region Alert
    private void Alert()
    {
        NavMeshAgent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer <= AttackRadius.radius)
        {
            currentEnemyState = EnemyState.ATTACK; //Attack if within attacking radius
        }
        else if (distanceToPlayer > DetectionRadius.radius)
        {
            currentEnemyState = EnemyState.PATROL; //Go back to patrolling if player goes out of sight
            NavMeshAgent.SetDestination(patrolPoints[0].position);
            currentPatrolPointIndex = 0;
            transform.LookAt(patrolPoints[0].position);
        }
        else
        {
            NavMeshAgent.SetDestination(Player.transform.position); //Chase
            transform.LookAt(Player.transform);
        }
    }
    #endregion

    #region Attack
    private void Attack()
    {
        NavMeshAgent.isStopped = true;
        transform.LookAt(Player.transform);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            //Perform attack animation
            Debug.Log("Enemy attacks player!");
            //Call a method on the player to deal damage here
            //Player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);

            lastAttackTime = Time.time;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer > AttackRadius.radius)
        {
            currentEnemyState = EnemyState.ALERT;
        }
    }
    #endregion
}
