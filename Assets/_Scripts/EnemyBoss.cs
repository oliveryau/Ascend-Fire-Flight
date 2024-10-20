using System.Collections;
using UnityEngine;

public class EnemyBoss : EnemyController
{
    [Header("Boss Variables")]
    public BossState currentBossState;
    public enum BossState { WAIT, PHASEONE, PHASETWO };

    public GameObject projectile;
    public Transform projectileFirepoint;
    public float shootForce;

    [Header("Boss Spawners")]
    public EnemyBossMeleeSpawner[] meleeSpawners;

    private void Start()
    {
        enemyId = 99;
        InitializeEnemy();
        InitializeBoss();
    }

    private void Update()
    {
        CheckEnemyState();
    }

    private void ChangeBossState(BossState nextState)
    {
        currentBossState = nextState;
    }

    private void InitializeBoss()
    {
        //Display boss hp ui
        meleeSpawners = FindObjectsByType<EnemyBossMeleeSpawner>(FindObjectsSortMode.None);
    }

    #region Wait
    public override void Wait()
    {
        StartCoroutine(WakeUp());
    }

    private IEnumerator WakeUp()
    {
        //AudioManager.Instance.PlayOneShot("Boss Wake Up", gameObject);
        yield return new WaitForSeconds(2f);
        ChangeEnemyState(EnemyState.ALERT);
    }
    #endregion

    #region Alert
    public override void Alert()
    {
        BodyCollider.enabled = true;

        ChangeEnemyState(EnemyState.ATTACK);
        ChangeBossState(BossState.PHASEONE);
    }
    #endregion

    #region Attack
    public override void Attacking()
    {
        switch (currentBossState)
        {
            case BossState.PHASEONE:
                ActivateSpawners();
                CheckSpawners();
                break;
            case BossState.PHASETWO:
                //RangedAttack();
                break;
        }

        //if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        //{
        //    Debug.LogWarning("Enemy attacking player!");
        //    StartCoroutine(PerformAttack());
        //}
    }

    private void ActivateSpawners()
    {
        foreach (var spawner in meleeSpawners)
        {
            spawner.SpawnMeleeEnemies();
        }
    }

    private void CheckSpawners()
    {
        foreach (var spawner in meleeSpawners)
        {
            if (spawner.gameObject.activeSelf) return;
        }

        ChangeBossState(BossState.PHASETWO);
    }

    //private void RangedAttack()
    //{
    //    if (gameObject.layer != LayerMask.NameToLayer("Enemy")) 
    //    {
    //        gameObject.layer = LayerMask.NameToLayer("Enemy"); //For enemy indicator
    //    }
    //    if (!isAttacking)
    //    {
    //        StartCoroutine(PerformRangedAttack());
    //    }
    //}

    //private IEnumerator PerformRangedAttack()
    //{
    //    isAttacking = true;
    //    Debug.LogWarning("Ranged Attacking");
    //    Vector3 shootDirection = (Player.transform.position - projectileFirepoint.position).normalized;

    //    GameObject bulletProjectile = Instantiate(projectile, projectileFirepoint.position, projectileFirepoint.rotation);
    //    bulletProjectile.GetComponent<Rigidbody>().AddForce(shootDirection * shootForce, ForceMode.Impulse);

    //    //Animator.SetTrigger("Ranged");
    //    yield return new WaitForSeconds(0.5f); //Delay before hitting player
    //    Debug.LogError("RANGED!");
    //    lastAttackTime = Time.time;
    //    yield return new WaitForSeconds(attackCooldown);
    //    isAttacking = false;
    //}
    #endregion

    #region Take Damage
    public override void TakeDamage(float damageTaken)
    {
        if (currentBossState == BossState.PHASEONE)
        {
            //Animator.SetTrigger("Null Damage");
            //AudioManager.Instance.PlayOneShot("Boss Null Damage", gameObject);
            return;
        }

        base.TakeDamage(damageTaken);
    }
    #endregion

    #region Death
    public override void Dead()
    {
        base.Dead();
    }
    #endregion
}
