using System.Collections;
using UnityEngine;

public class EnemyBoss : EnemyController
{
    #region Variables
    [Header("Boss Variables")]
    public BossState currentBossState;
    public enum BossState { WAIT, PHASEONE, PHASETWO };

    public GameObject projectile;
    public Transform projectileFirepoint;
    public float shootForce;
    public float spreadAngle;

    [Header("References")]
    private EnemyBossMeleeSpawner[] meleeSpawners;
    private EnemyBossRangedSpawner[] rangedSpawners;
    private EnemyBossPlatformWave[] platformWaves;
    private Coroutine waveCoroutine;
    private UiManager UiManager;
    private bool isPlatformSequenceActive;
    #endregion

    #region Initialization
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
        UiManager = FindFirstObjectByType<UiManager>();

        meleeSpawners = FindObjectsByType<EnemyBossMeleeSpawner>(FindObjectsSortMode.None);
        rangedSpawners = FindObjectsByType<EnemyBossRangedSpawner>(FindObjectsSortMode.None);
        platformWaves = FindObjectsByType<EnemyBossPlatformWave>(FindObjectsSortMode.None);
    }
    #endregion

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
        ChangeBossState(BossState.PHASETWO);
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

        if (currentHealth > 0) ActivateBossHealth();
    }

    private void ActivateBossHealth()
    {
        UiManager.enemyBossHealthUi.SetActive(true);
        UiManager.UpdateBossEnemyHealthBar(this);
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
        if (!isPlatformSequenceActive) yield break; //Exit if platform sequence inactive
        if (currentHealth <= 0) yield break;

        for (int i = 0; i < platformWaves.Length; i++)
        {
            waveCoroutine = StartCoroutine(ActivatePlatformWave(platformWaves[i]));
            yield return new WaitUntil(() => platformWaves[i].hasRespawned);
            platformWaves[i].ResetWave();
            StopCoroutine(waveCoroutine);
        }
    }

    private IEnumerator ActivatePlatformWave(EnemyBossPlatformWave platformWave)
    {
        if (platformWave == null) yield break; //Exit if no platform wave

        FallingPlatform[] platforms = platformWave.GetComponentsInChildren<FallingPlatform>();
        foreach (FallingPlatform platform in platforms)
        {
            if (currentHealth <= 0) yield break;
            platform.StartShaking(platformWave.gameObject, platformWave.fallDelay, platform, platformWave.stayDelay);
        }

        while (!platformWave.hasRespawned)
        {
            if (currentHealth <= 0) yield break;
            yield return null;
        }

        yield return new WaitForSeconds(platformWave.respawnDelay);
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
        base.Dead();
    }

    public override IEnumerator DeathAnimation()
    {
        foreach (var spawner in rangedSpawners)
        {
            StartCoroutine(spawner.SpawnerDestroy());
        }
        isPlatformSequenceActive = false;

        UiManager.enemyBossHealthUi.SetActive(false); 
        Animator.SetTrigger("Death");
        //AudioManager.Instance.PlayOneShot("Enemy Boss Death", gameObject);
        GetComponent<SphereCollider>().enabled = false;

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    #endregion
}
