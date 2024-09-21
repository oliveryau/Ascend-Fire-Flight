using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    #region Variables
    public enum EnemyState { WAIT, PATROL, ALERT, ATTACK, DEAD }
    public EnemyState currentEnemyState;

    [Header("Enemy Base Variables")]
    public int enemyId;
    public int maxHealth;
    public int currentHealth;

    [Header("Enemy Patrol Points")]
    public Transform[] patrolPoints;
    protected int currentPatrolPointIndex;

    [Header("Enemy Attack Variables")]
    public int attackDamage;
    public float attackCooldown;
    protected bool isAttacking;
    protected float lastAttackTime;

    [Header("References")]
    public CapsuleCollider DetectionRadius;
    public SphereCollider AttackRadius;
    protected PlayerController Player;
    protected NavMeshAgent NavMeshAgent;
    private Animator Animator;
    #endregion

    #region State Control
    public void InitializeEnemy()
    {
        currentEnemyState = EnemyState.PATROL;

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
                Attacking();
                break;
            case EnemyState.DEAD:
                Dead();
                break;
        }
    }

    public void ChangeEnemyState(EnemyState newState)
    {
        currentEnemyState = newState;
    }

    private void LookAtPlayer()
    {
        Vector3 direction = Player.transform.position - transform.position;
        direction.y = 0; // This ensures rotation only around the y-axis

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            Vector3 currentRotation = transform.rotation.eulerAngles;
            Vector3 targetEuler = targetRotation.eulerAngles;
            Quaternion newRotation = Quaternion.Euler(0, targetEuler.y, 0);

            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 10f);
        }
    }
    #endregion

    #region Patrol
    public virtual void Patrol()
    {
        if (patrolPoints.Length <= 1)
        {
            if (Vector3.Distance(transform.position, patrolPoints[0].position) > 0.1f)
            {
                NavMeshAgent.isStopped = false;
                NavMeshAgent.SetDestination(patrolPoints[0].position); //Move back to patrol point if not there
            }
            else
            {
                NavMeshAgent.isStopped = true;
            }
        }
        else
        {
            NavMeshAgent.isStopped = false;

            if (NavMeshAgent.remainingDistance < 0.1f)
            {
                currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
                NavMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
                transform.LookAt(patrolPoints[currentPatrolPointIndex].position);
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer <= DetectionRadius.radius)
        {
            ChangeEnemyState(EnemyState.ALERT);
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
            ChangeEnemyState(EnemyState.ATTACK); //Attack if within attacking radius
        }
        else if (distanceToPlayer > DetectionRadius.radius)
        {
            ChangeEnemyState(EnemyState.PATROL); //Go back to patrolling if player goes out of sight
            NavMeshAgent.SetDestination(patrolPoints[0].position);
            currentPatrolPointIndex = 0;
            transform.LookAt(patrolPoints[0].position);
        }
        else
        {
            NavMeshAgent.SetDestination(Player.transform.position); //Chase
            transform.LookAt(Player.transform);
            LookAtPlayer();
        }
    }
    #endregion

    #region Attack
    public virtual void Attacking()
    {
        NavMeshAgent.isStopped = true;
        LookAtPlayer();

        if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        {
            Debug.LogWarning("Enemy attacking player!");
            StartCoroutine(PerformAttack());
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer > AttackRadius.radius)
        {
            ChangeEnemyState(EnemyState.ALERT);
            lastAttackTime -= attackCooldown;
        }
    }

    public virtual IEnumerator PerformAttack()
    {
        yield return null;
        //Child methods
    }
    #endregion

    #region Taking Damage
    public void TakeDamage(int damageTaken)
    {
        if (currentEnemyState == EnemyState.DEAD) return;

        currentHealth -= damageTaken;
        //Trigger damage effects or animations here

        if (currentHealth <= 0)
        {
            ChangeEnemyState(EnemyState.DEAD);
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
        //ragdoll
        //disable collider
        //disappear after 1/2 sec
        gameObject.SetActive(false);
    }
    #endregion
}
