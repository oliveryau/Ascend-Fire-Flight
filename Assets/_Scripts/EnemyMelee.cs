using System.Collections;
using UnityEngine;

public class EnemyMelee : EnemyController
{
    [Header("Melee Enemy Variables")]
    public GameObject meleeSlashParticle;

    private void Start()
    {
        enemyId = 1;
        InitializeEnemy();
    }

    private void Update()
    {
        CheckEnemyState();
    }

    #region Wait
    public override void Wait()
    {
        base.Wait();
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
        Animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f); //Delay before hitting player
        meleeSlashParticle.SetActive(true);
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(0.5f);
        meleeSlashParticle.SetActive(false);
        yield return new WaitForSeconds(attackCooldown - 0.5f);
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
