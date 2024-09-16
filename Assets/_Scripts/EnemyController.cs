using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static PlayerController;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { WAIT, PATROL, ALERT, ATTACK, DEAD }
    public EnemyState currentEnemyState;

    [Header("Enemy Base Variables")]
    public int enemyId;
    public float maxHealth;
    public float currentHealth;

    [Header("Enemy Patrol Points")]
    public Transform[] patrolPoints;
    private int currentPatrolPointIndex;

    [Header("Enemy Attack Variables")]
    public float attackDamage;
    public float attackCooldown;
    private bool isAttacking;
    private float lastAttackTime;

    [Header("References")]
    public CapsuleCollider DetectionRadius;
    public SphereCollider AttackRadius;
    private PlayerController Player;
    private NavMeshAgent NavMeshAgent;
    private Animator Animator;

    public void InitializeEnemy()
    {
        currentEnemyState = EnemyState.ALERT;

        Player = FindFirstObjectByType<PlayerController>();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();

        if (patrolPoints.Length > 0) NavMeshAgent.SetDestination(patrolPoints[0].position);

        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown;
    }

    public void CheckEnemyState()
    {
        switch (currentEnemyState)
        {
            case EnemyState.WAIT:
                break;
            case EnemyState.PATROL:
                Patrol(); //Patrol/Idle
                break;
            case EnemyState.ALERT:
                Alert(); //Move towards player
                break;
            case EnemyState.ATTACK:
                Attack();
                break;
            case EnemyState.DEAD:
                Dead();
                break;
        }
    }

    #region Patrol
    public virtual void Patrol()
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
    public virtual void Alert()
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
    public virtual void Attack()
    {
        NavMeshAgent.isStopped = true;
        transform.LookAt(Player.transform);

        if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        {
            Debug.LogWarning("Enemy attacking player!");
            StartCoroutine(PerformAttackAnimation());
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer > AttackRadius.radius)
        {
            currentEnemyState = EnemyState.ALERT;
            lastAttackTime -= attackCooldown;
        }
    }

    public virtual IEnumerator PerformAttackAnimation()
    {
        isAttacking = true;
        //Animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f); //Delay before hitting player
        Debug.LogError("Enemy attacks player!");
        Player.GetComponent<PlayerController>().TakeDamage(attackDamage);
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(attackCooldown - 0.5f);
        isAttacking = false;
    }
    #endregion

    #region Taking Damage
    public void TakeDamage(float damageTaken)
    {
        if (currentEnemyState == EnemyState.DEAD) return;

        currentHealth -= damageTaken;
        //Trigger damage effects or animations here

        if (currentHealth <= 0)
        {
            currentEnemyState = EnemyState.DEAD;
        }
        else if (currentHealth < maxHealth * 0.3f)
        {
            //Play low health warning effects
        }
    }
    #endregion

    #region Death
    public virtual void Dead()
    {
        gameObject.SetActive(false);
    }
    #endregion
}
