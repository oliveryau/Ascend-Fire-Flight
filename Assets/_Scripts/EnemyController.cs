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
    public float maxHealth;
    public float currentHealth;

    private bool playedSpawnSound;
    private bool playedDeathSound;

    [Header("Enemy Patrol Points")]
    public Transform[] patrolPoints;

    [SerializeField]protected int currentPatrolPointIndex;

    [Header("Enemy Attack Variables")]
    public float attackDamage;
    public float attackCooldown;

    protected bool isAttacking;
    protected float lastAttackTime;

    [Header("References")]
    public GameObject mainAuraParticle;
    public GameObject damagedParticle;
    public SphereCollider DetectionRadius;
    public SphereCollider AttackRadius;
    protected PlayerController Player;
    protected SphereCollider BodyCollider;
    protected NavMeshAgent NavMeshAgent;
    protected Animator Animator;
    #endregion

    #region State Control
    public void InitializeEnemy()
    {
        currentEnemyState = EnemyState.WAIT;

        Player = FindFirstObjectByType<PlayerController>();
        BodyCollider = GetComponent<SphereCollider>();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();

        if (patrolPoints.Length > 0) NavMeshAgent.SetDestination(patrolPoints[0].position);

        BodyCollider.enabled = false;
        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown;
    }

    public void CheckEnemyState()
    {
        switch (currentEnemyState)
        {
            case EnemyState.WAIT:
                Wait();
                break;
            case EnemyState.PATROL:
                Patrol();
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

    public void LookAtPlayer()
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

    #region Wait
    public virtual void Wait()
    {
        if (currentHealth <= 0) return;

        StartCoroutine(Spawning());
    }

    private IEnumerator Spawning()
    {
        NavMeshAgent.isStopped = true;
        RandomiseSpawnAudio();
        yield return new WaitForSeconds(1f);
        LookAtPlayer();
        yield return new WaitForSeconds(1f);
        ChangeEnemyState(EnemyState.PATROL);
    }

    private void RandomiseSpawnAudio()
    {
        if (!playedSpawnSound)
        {
            playedSpawnSound = true;
            int soundIndex = Random.Range(1, 3);
            string soundName = $"Enemy Spawn {soundIndex}";
            AudioManager.Instance.PlayOneShot(soundName, gameObject);
        }
    }
    #endregion

    #region Patrol
    public virtual void Patrol()
    {
        if (DetectionRadius == null)
        {
            NavMeshAgent.isStopped = false;
            Animator.SetBool("Walking", false);
            return;
        }

        if (patrolPoints.Length > 1) //Patrol if more than 2 points
        {
            NavMeshAgent.isStopped = false;
            Animator.SetBool("Walking", true);

            if (NavMeshAgent.remainingDistance <= 0.1f)
            {
                Animator.SetBool("Walking", false);
                currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
                NavMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, patrolPoints[currentPatrolPointIndex].rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
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
        if (currentHealth <= 0) return;
        
        BodyCollider.enabled = true;
        NavMeshAgent.isStopped = false;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer <= AttackRadius.radius)
        {
            Animator.SetBool("Walking", false);
            ChangeEnemyState(EnemyState.ATTACK); //Attack if within attacking radius
        }
        else if (distanceToPlayer > AttackRadius.radius && distanceToPlayer <= DetectionRadius.radius)
        {
            NavMeshAgent.SetDestination(Player.transform.position); //Chase
            LookAtPlayer();
            Animator.SetBool("Walking", true);
        }
        else if (distanceToPlayer > DetectionRadius.radius)
        {
            ChangeEnemyState(EnemyState.PATROL); //Go back to patrolling if player goes out of sight
            NavMeshAgent.SetDestination(patrolPoints[0].position);
            currentPatrolPointIndex = 0;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, patrolPoints[currentPatrolPointIndex].rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }
    #endregion

    #region Attack
    public virtual void Attacking()
    {
        if (currentHealth <= 0) return;

        NavMeshAgent.isStopped = true;
        NavMeshAgent.velocity = Vector3.zero;
        LookAtPlayer();

        if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        {
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
    public virtual void TakeDamage(float damageTaken)
    {
        if (currentEnemyState == EnemyState.DEAD) return;

        currentHealth -= damageTaken;
        Animator.SetTrigger("Damaged");

        if (currentHealth <= 0)
        {
            ChangeEnemyState(EnemyState.DEAD);
        }
        else
        {
            RandomiseDamagedAudio();
        }
    }

    private void RandomiseDamagedAudio()
    {
        int soundIndex = Random.Range(1, 4);
        string soundName = $"Enemy Damaged {soundIndex}";
        AudioManager.Instance.PlayOneShot(soundName, gameObject);
    }
    #endregion

    #region Death
    public virtual void Dead()
    {
        StartCoroutine(DeathAnimation());
    }

    public virtual IEnumerator DeathAnimation()
    {
        NavMeshAgent.isStopped = true;
        NavMeshAgent.velocity = Vector3.zero;
        Animator.SetBool("Walking", false);
        Animator.SetBool("Attack", false);
        Animator.SetTrigger("Death");
        RandomiseDeathAudio();
        GetComponent<SphereCollider>().enabled = false;

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private void RandomiseDeathAudio()
    {
        if (!playedDeathSound)
        {
            playedDeathSound = true;
            int soundIndex = Random.Range(1, 3);
            string soundName = $"Enemy Death {soundIndex}";
            AudioManager.Instance.PlayOneShot(soundName, gameObject);
        }
    }
    #endregion
}
