using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    #region Variables
    public enum EnemyState { WAIT, ALERT, ATTACK, DEAD }
    public EnemyState currentEnemyState;

    [Header("Enemy Base Variables")]
    public int enemyId;
    public float maxHealth;
    public float currentHealth;

    private bool playedSpawnSound;
    private bool playedDeathSound;

    [Header("Enemy Attack Variables")]
    public float attackDamage;
    public float attackCooldown;

    protected bool isAttacking;
    protected float lastAttackTime;

    [Header("References")]
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
        RandomiseSpawnAudio();
        yield return new WaitForSeconds(0.5f);
        LookAtPlayer();
        yield return new WaitForSeconds(0.5f);
        ChangeEnemyState(EnemyState.ALERT);
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
        else
        {
            NavMeshAgent.SetDestination(Player.transform.position); //Chase
            LookAtPlayer();
            Animator.SetBool("Walking", true);
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
    }
    #endregion

    #region Death
    public virtual void Dead()
    {
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        NavMeshAgent.isStopped = true;
        NavMeshAgent.velocity = Vector3.zero;
        Animator.SetTrigger("Death");
        RandomiseDeathAudio();
        GetComponent<SphereCollider>().enabled = false;

        yield return new WaitForSeconds(1f);
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
