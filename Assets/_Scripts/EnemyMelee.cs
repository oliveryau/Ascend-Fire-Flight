using System.Collections;
using UnityEngine;

public class EnemyMelee : EnemyController
{
    private void Start()
    {
        enemyId = 1;
        InitializeEnemy();
    }

    private void Update()
    {
        CheckEnemyState();
    }

    #region Patrol
    public override void Patrol()
    {
        base.Patrol();
    }
    #endregion

    #region Alert
    public override void Alert()
    {
        base.Alert();
    }
    #endregion

    #region Attack
    public override void Attacking()
    {
        base.Attacking();
    }

    public override IEnumerator PerformAttack()
    {
        isAttacking = true;
        //Animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f); //Delay before hitting player
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
