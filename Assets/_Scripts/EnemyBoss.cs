using System.Collections;
using UnityEngine;

public class EnemyBoss : EnemyController
{
    [Header("Miniboss Attack State")]
    public MinibossAttack currentAttack;
    public enum MinibossAttack { CHASE, CHARGE, MELEE, RANGED };

    [Header("Miniboss Charge Variables")]
    public int chargeSpeed;
    public float chargeDuration;
    public int chargeDamage;

    private bool isCharging;
    private Vector3 chargeDirection;

    [Header("Miniboss Ranged Variables")]
    public GameObject projectile;
    public Transform projectileFirepoint;
    public float shootForce;

    [Header("Miniboss References")]
    public SphereCollider MeleeAttackRadius;
    public SphereCollider RangedAttackRadius;

    private void Start()
    {
        enemyId = 98;
        InitializeEnemy();
    }

    private void Update()
    {
        CheckEnemyState();
        BossAttack();
    }

    private void ChangeBossAttack(MinibossAttack nextAttack)
    {
        currentAttack = nextAttack;
    }

    #region Wait
    public override void Wait()
    {
        //Regain health if less than current health (may not need)

        base.Wait();
    }
    #endregion

    #region Alert
    public override void Alert()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer <= AttackRadius.radius)
        {
            //Animator.SetTrigger("Idle");
            ChangeEnemyState(EnemyState.ATTACK); //Attack if within attacking radius
        }
    }
    #endregion

    #region Attack
    public override void Attacking()
    {
        LookAtPlayer();

        //float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        //if (Player.isGrounded && distanceToPlayer > MeleeAttackRadius.radius)
        //{
        //    ChangeBossAttack(MinibossAttack.CHARGE);
        //}
        //else if (Player.isGrounded && distanceToPlayer <= MeleeAttackRadius.radius)
        //{
        //    ChangeBossAttack(MinibossAttack.MELEE);
        //}
        //else if (!Player.isGrounded && distanceToPlayer < RangedAttackRadius.radius)
        //{
        //    ChangeBossAttack(MinibossAttack.RANGED);
        //}
        //else
        //{
        //    ChangeBossAttack(MinibossAttack.CHASE);
        //}

        //if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        //{
        //    Debug.LogWarning("Enemy attacking player!");
        //    StartCoroutine(PerformAttack());
        //}
    }

    private void BossAttack()
    {
        if (currentEnemyState != EnemyState.ATTACK) return;

        switch (currentAttack)
        {
            case MinibossAttack.CHASE:
                Chase();
                break;
            case MinibossAttack.CHARGE:
                Charge();
                break;
            case MinibossAttack.MELEE:
                Melee();
                break;
            case MinibossAttack.RANGED:
                Ranged();
                break;
        }
    }

    private void Chase()
    {
        NavMeshAgent.isStopped = false;

        NavMeshAgent.SetDestination(Player.transform.position);
        LookAtPlayer();
    }

    private void Charge()
    {
        if (!isCharging)
        {
            StartCoroutine(PerformChargeAttack());
        }
    }

    private IEnumerator PerformChargeAttack()
    {
        NavMeshAgent.isStopped = true;
        isCharging = true;
        Debug.LogWarning("Charging Up");
        //Animator.SetTrigger("Charge")
        chargeDirection = (Player.transform.position - transform.position).normalized;

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        yield return new WaitForSeconds(1.5f);
        Debug.LogError("CHARGE!");
        while (elapsedTime < chargeDuration)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            //Activate emitter for trail

            // Check for collision with player
            if (Vector3.Distance(transform.position, Player.transform.position) < 0.5f)
            {
                Player.GetComponent<PlayerController>().TakeDamage(chargeDamage);
                break;
            }

            yield return null;
        }

        isCharging = false;
        NavMeshAgent.isStopped = false;

        yield return new WaitForSeconds(1.5f);
    }

    private void Melee()
    {
        if (!isAttacking)
        {
            StartCoroutine(PerformMeleeAttack());
        }
    }

    private IEnumerator PerformMeleeAttack()
    {
        NavMeshAgent.isStopped = true;
        isAttacking = true;
        Debug.LogWarning("Melee Attacking");
        //Animator.SetTrigger("Melee");
        yield return new WaitForSeconds(0.5f); //Delay before hitting player
        Debug.LogError("MELEE!");
        Player.GetComponent<PlayerController>().TakeDamage(attackDamage);
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void Ranged()
    {
        if (!isAttacking)
        {
            StartCoroutine(PerformRangedAttack());
        }
    }

    private IEnumerator PerformRangedAttack()
    {
        NavMeshAgent.isStopped = true;
        isAttacking = true;
        Debug.LogWarning("Ranged Attacking");
        Vector3 shootDirection = (Player.transform.position - projectileFirepoint.position).normalized;

        GameObject bulletProjectile = Instantiate(projectile, projectileFirepoint.position, projectileFirepoint.rotation);
        bulletProjectile.GetComponent<Rigidbody>().AddForce(shootDirection * shootForce, ForceMode.Impulse);

        //Animator.SetTrigger("Ranged");
        yield return new WaitForSeconds(0.5f); //Delay before hitting player
        Debug.LogError("RANGED!");
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }
    #endregion

    #region Death
    public override void Dead()
    {
        base.Dead();
    }
    #endregion
}
