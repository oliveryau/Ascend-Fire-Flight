using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    [Header("Phase Variables")]
    public DialogueTriggerEvent dialogueTriggerEventPhaseOne;
    public DialogueTriggerEvent dialogueTriggerEventPhaseTwo;
    public float phaseOneTimerInterval;
    public float phaseTwoTimerInterval;
    public GameObject phaseTwoParticle;
    public GameObject deathParticle;

    [SerializeField] private float phaseOneTimer;
    [SerializeField] private float phaseTwoTimer;

    [Header("References")]
    public GameObject[] weakPoints;
    [SerializeField] private GameObject[] flamePlatforms;
    [SerializeField] private EnemyBossMeleeSpawner[] meleeSpawners;
    [SerializeField] private EnemyBossRangedSpawner rangedSpawner;
    [SerializeField] private EnemyBossFallingPlatformTrigger[] bossPlatformTriggers;
    private UiManager UiManager;
    private BoxCollider BoxCollider;

    private bool spawnSoundPlaying;
    private bool flameSoundPlaying;
    private bool deathPlaying;
    #endregion

    #region Initialization
    private void Start()
    {
        enemyId = 99;
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
        currentEnemyState = EnemyState.WAIT;

        Player = FindFirstObjectByType<PlayerController>();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();

        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown;
        UiManager = FindFirstObjectByType<UiManager>();
        BoxCollider = GetComponent<BoxCollider>();

        BoxCollider.enabled = false;
        flamePlatforms = GameObject.FindGameObjectsWithTag("Lava");
        meleeSpawners = FindObjectsByType<EnemyBossMeleeSpawner>(FindObjectsSortMode.None);
        rangedSpawner = GetComponent<EnemyBossRangedSpawner>();
        bossPlatformTriggers = FindObjectsByType<EnemyBossFallingPlatformTrigger>(FindObjectsSortMode.None);

        phaseOneTimer = phaseOneTimerInterval - 1f;
        phaseTwoTimer = phaseTwoTimerInterval - 1f;
    }
    #endregion

    #region Wait
    public override void Wait()
    {
        StartCoroutine(WakeUp());
    }

    private IEnumerator WakeUp()
    {
        if (!spawnSoundPlaying)
        {
            AudioManager.Instance.PlayOneShot("Enemy Boss Spawn", gameObject);
            spawnSoundPlaying = true;
        }

        yield return new WaitForSeconds(3f);
        ChangeEnemyState(EnemyState.ALERT);
    }
    #endregion

    #region Patrol
    public override void Patrol()
    {
        ChangeEnemyState(EnemyState.ALERT);
    }
    #endregion

    #region Alert
    public override void Alert()
    {
        BoxCollider.enabled = true;

        ChangeEnemyState(EnemyState.ATTACK);
        ChangeBossState(BossState.PHASEONE);
    }
    #endregion

    #region Attack
    public override void Attacking()
    {
        switch (currentBossState)
        {
            case BossState.WAIT:
                break;
            case BossState.PHASEONE:
                ActivateMeleeSpawners();
                CheckMeleeSpawners();
                UpdatePhaseOneTimer();
                break;
            case BossState.PHASETWO:
                SwitchToEnemyLayer();
                ActivateRangedSpawners();
                ActivateFallingPlatformSequence();
                RangedAttack();
                UpdatePhaseTwoTimer();
                break;
        }

        if (currentHealth > 0)
        {
            ActivateBossHealth();
            ActivateFlames();
        }
    }

    private void ActivateBossHealth()
    {
        UiManager.enemyBossHealthUi.SetActive(true);
        UiManager.UpdateBossEnemyHealthBar(this);
    }

    private void ActivateFlames()
    {
        foreach (var platform in flamePlatforms)
        {
            ParticleSystem fire = platform.GetComponentInChildren<ParticleSystem>();
            if (fire.isPlaying) return;
            if (!fire.isPlaying) fire.Play();

            if (!flameSoundPlaying)
            {
                AudioManager.Instance.PlayOneShot("Fire Start", platform.gameObject);
                AudioManager.Instance.Play("Fire Burning", platform.gameObject);
                flameSoundPlaying = true;
            }
        }
    }

    #region Phase 1
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

        StartCoroutine(ChangeToBossPhaseTwo());
    }

    private void UpdatePhaseOneTimer()
    {
        phaseOneTimer += Time.deltaTime;

        if (phaseOneTimer >= 10f)
        {
            dialogueTriggerEventPhaseOne.hasTriggered = false;
        }

        if (phaseOneTimer >= phaseOneTimerInterval)
        {
            dialogueTriggerEventPhaseOne.TriggerDialogue();
            phaseOneTimer = 0f;
        }
    }

    private IEnumerator ChangeToBossPhaseTwo()
    {
        Animator.SetTrigger("Phase Two");
        phaseTwoParticle.SetActive(true);
        AudioManager.Instance.PlayOneShot("Enemy Change Phase", gameObject);
        yield return new WaitForSeconds(2f);
        ChangeBossState(BossState.PHASETWO);
    }
    #endregion

    #region Phase 2
    private void SwitchToEnemyLayer()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Enemy")) return;

        mainAuraParticle.SetActive(true);
        gameObject.layer = LayerMask.NameToLayer("Enemy"); //For enemy indicator
    }

    private void ActivateRangedSpawners()
    {
        rangedSpawner.SpawnRangedEnemies();
    }

    private void ActivateFallingPlatformSequence()
    {
        foreach (var platformTrigger in bossPlatformTriggers)
        {
            platformTrigger.GetComponent<BoxCollider>().enabled = true;
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
        foreach (var point in weakPoints) point.SetActive(true);
        Animator.SetTrigger("Attack");
        AudioManager.Instance.PlayOneShot("Enemy Boss Windup", gameObject);
        yield return new WaitForSeconds(2f);

        foreach (var point in weakPoints) point.SetActive(false);
        Vector3 baseShootDirection = (Player.transform.position - projectileFirepoint.position).normalized;
        Vector3 spreadDirection = ApplySpread(baseShootDirection);

        GameObject firstBulletProjectile = Instantiate(projectile, projectileFirepoint.position, projectileFirepoint.rotation);
        firstBulletProjectile.GetComponent<Rigidbody>().AddForce(spreadDirection * shootForce, ForceMode.Impulse);
        AudioManager.Instance.PlayOneShot("Enemy Boss Attack", gameObject);
        yield return new WaitForSeconds(0.3f); //Short interval

        GameObject secondBulletProjectile = Instantiate(projectile, projectileFirepoint.position, projectileFirepoint.rotation);
        secondBulletProjectile.GetComponent<Rigidbody>().AddForce(spreadDirection * shootForce, ForceMode.Impulse);
        AudioManager.Instance.PlayOneShot("Enemy Boss Attack", gameObject);

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

    private void UpdatePhaseTwoTimer()
    {
        phaseTwoTimer += Time.deltaTime;
        if (phaseTwoTimer >= phaseTwoTimerInterval)
        {
            dialogueTriggerEventPhaseTwo.TriggerDialogue();
            phaseTwoTimer = 0f;
        }
    }
    #endregion
    #endregion

    #region Take Damage
    public override void TakeDamage(float damageTaken)
    {
        if (currentEnemyState == EnemyState.DEAD) return;

        if (currentBossState == BossState.PHASEONE)
        {
            FlashHealthBar("Null");
            //AudioManager.Instance.PlayOneShot("Boss Null Damage", gameObject);
            return;
        }
        else
        {
            base.TakeDamage(damageTaken);
        }

    }

    public void FlashHealthBar(string animName)
    {
        UiManager.FlashBossEnemyHealthBar(animName);
    }
    #endregion

    #region Death
    public override void Dead()
    {
        base.Dead();
    }

    public override IEnumerator DeathAnimation()
    {
        if (rangedSpawner.isActivated) rangedSpawner.SpawnerStop();
        foreach (var platformTrigger in bossPlatformTriggers) platformTrigger.GetComponent<BoxCollider>().enabled = false;

        UiManager.enemyBossHealthUi.SetActive(false); 
        Animator.SetTrigger("Death");
        BoxCollider.enabled = false;
        deathParticle.SetActive(true);
        mainAuraParticle.SetActive(false);

        if (!deathPlaying)
        {
            AudioManager.Instance.PlayOneShot("Enemy Boss Death", gameObject);
            AudioManager.Instance.Play("Boss End BGM");
            AudioManager.Instance.FadeOut("Boss BGM", 10f);
            deathPlaying = true;
        }

        yield return new WaitForSeconds(3f);
        AudioManager.Instance.FadeIn("Main BGM", 10f);
        Destroy(gameObject);
    }
    #endregion
}
