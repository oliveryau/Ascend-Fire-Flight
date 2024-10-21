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
    public float spreadAngle;

    [Header("Boss Spawners")]
    public EnemyBossMeleeSpawner[] meleeSpawners;
    public EnemyBossRangedSpawner[] rangedSpawners;

    [Header("Boss Platforms")]
    public EnemyBossPlatform[] platformWaves;

    private bool isPlatformSequenceActive;

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
        rangedSpawners = FindObjectsByType<EnemyBossRangedSpawner>(FindObjectsSortMode.None);
        platformWaves = FindObjectsByType<EnemyBossPlatform>(FindObjectsSortMode.None);
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
                ActivateMeleeSpawners();
                CheckMeleeSpawners();
                break;
            case BossState.PHASETWO:
                SwitchToEnemyLayer();
                ActivateRangedSpawners();
                StartFallingPlatformWaves();
                RangedAttack();
                break;
        }
    }

    private void ActivateMeleeSpawners()
    {
        foreach (var spawner in meleeSpawners)
        {
            spawner.SpawnMeleeEnemies();
        }
    }

    private void CheckMeleeSpawners()
    {
        foreach (var spawner in meleeSpawners)
        {
            if (spawner.gameObject.activeSelf) return;
        }

        ChangeBossState(BossState.PHASETWO);
    }

    private void SwitchToEnemyLayer()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Enemy")) return;

        gameObject.layer = LayerMask.NameToLayer("Enemy"); //For enemy indicator
    }

    private void ActivateRangedSpawners()
    {
        foreach (var spawner in rangedSpawners)
        {
            spawner.GetComponent<MeshRenderer>().enabled = true;
            spawner.GetComponent<BoxCollider>().enabled = true;
            spawner.SpawnRangedEnemies();
        }
    }

    private void StartFallingPlatformWaves()
    {
        isPlatformSequenceActive = true;
        
        StartCoroutine(FallingPlatformSequence());
    }

    private IEnumerator FallingPlatformSequence()
    {
        if (!isPlatformSequenceActive) yield break;

        while (currentHealth > 0)
        {
            foreach (EnemyBossPlatform platformWave in platformWaves)
            {
                if (currentHealth <= 0) yield break; // Exit if boss dies during sequence

                yield return StartCoroutine(ActivatePlatformWave(platformWave));
                yield return new WaitForSeconds(platformWave.timeBetweenWaves);
            }
        }
    }

    private IEnumerator ActivatePlatformWave(EnemyBossPlatform platformWave)
    {
        if (platformWave == null) yield break;

        // Get all platforms in this wave
        FallingPlatform[] platforms = platformWave.GetComponentsInChildren<FallingPlatform>();

        foreach (FallingPlatform platform in platforms)
        {
            if (currentHealth <= 0) yield break; // Exit if boss dies during wave

            // Get or add the trigger component
            FallingPlatformTrigger trigger = platform.GetComponent<FallingPlatformTrigger>();
            if (trigger == null)
            {
                trigger = platform.gameObject.AddComponent<FallingPlatformTrigger>();
            }

            // Start the falling sequence
            StartCoroutine(platform.Falling(trigger, 1f)); // 1f is the fall delay
            yield return new WaitForSeconds(platform.destroyDelay);
        }
    }

    private void RangedAttack()
    {
        if (currentHealth <= 0) return;

        if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }

    public override IEnumerator PerformAttack()
    {
        isAttacking = true;
        //Animator.SetTrigger("Windup");
        //yield return new WaitForSeconds(2f);

        Vector3 baseShootDirection = (Player.transform.position - projectileFirepoint.position).normalized;
        Vector3 spreadDirection = ApplySpread(baseShootDirection);

        GameObject firstBulletProjectile = Instantiate(projectile, projectileFirepoint.position, projectileFirepoint.rotation);
        firstBulletProjectile.GetComponent<Rigidbody>().AddForce(spreadDirection * shootForce, ForceMode.Impulse);
        //Animator.SetTrigger("Attack");
        //AudioManager.Instance.PlayOneShot("Enemy Boss Ranged", gameObject);
        yield return new WaitForSeconds(0.3f); //Short interval

        GameObject secondBulletProjectile = Instantiate(projectile, projectileFirepoint.position, projectileFirepoint.rotation);
        secondBulletProjectile.GetComponent<Rigidbody>().AddForce(spreadDirection * shootForce, ForceMode.Impulse);
        //Animator.SetTrigger("Attack");
        //AudioManager.Instance.PlayOneShot("Enemy Boss Ranged", gameObject);

        yield return new WaitForSeconds(0.5f); //Delay before hitting player
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private Vector3 ApplySpread(Vector3 baseDirection)
    {
        float randomSpreadX = Random.Range(-spreadAngle, spreadAngle);
        float randomSpreadY = Random.Range(-spreadAngle, spreadAngle);

        Quaternion spreadRotation = Quaternion.Euler(randomSpreadY, randomSpreadX, 0);
        return spreadRotation * baseDirection;
    }
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
        isPlatformSequenceActive = false;
        base.Dead();
    }
    #endregion
}
