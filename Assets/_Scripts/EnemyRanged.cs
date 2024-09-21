using System.Collections;
using UnityEngine;

public class EnemyRanged : EnemyController
{
    [Header("Ranged Enemy Variables")]
    public GameObject bulletPrefab;
    public Transform bulletFirePoint;
    public float shootForce;

    private void Start()
    {
        enemyId = 2;
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
        Vector3 shootDirection = (Player.transform.position - bulletFirePoint.position).normalized;

        GameObject bulletProjectile = Instantiate(bulletPrefab, bulletFirePoint.position, bulletFirePoint.rotation);
        bulletProjectile.GetComponent<Rigidbody>().AddForce(shootDirection * shootForce, ForceMode.Impulse);
        //Animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f); //Delay before hitting player
        Debug.LogError("Ranged enemy attacks player!");
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
