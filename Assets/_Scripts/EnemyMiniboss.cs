using System.Collections;
using UnityEngine;

public class EnemyMiniboss : EnemyController
{
    public enum MinibossPhase {  };
    public MinibossPhase currentPhase;

    //public EnemySpawner[] enemySpawners;

    private void Start()
    {
        enemyId = 98;
        InitializeEnemy();
    }

    private void Update()
    {
        CheckEnemyState();
    }

    private void ChangeBossPhase()
    {
        currentPhase++;
    }

    #region Patrol
    public override void Patrol()
    {
        //Regain health

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer <= DetectionRadius.radius)
        {
            ChangeEnemyState(EnemyState.ALERT);
        }
    }
    #endregion

    #region Alert
    public override void Alert()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);
        if (distanceToPlayer <= AttackRadius.radius)
        {
            ChangeEnemyState(EnemyState.ATTACK); //Attack if within attacking radius
        }
    }
    #endregion

    #region Attack
    //Phase 1
    public override void Attacking()
    {
        //if (currentPhase == BossOnePhase.ONE)
        //{
        //    //Spawn enemy spawners
        //    //Freeze
        //}
    }

    public override IEnumerator PerformAttack()
    {
        isAttacking = true;
        //Animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f); //Delay before hitting player
        Debug.LogError("Melee enemy attacks player!");
        Player.GetComponent<PlayerController>().TakeDamage(attackDamage);
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
