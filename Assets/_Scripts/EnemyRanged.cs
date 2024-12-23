using System.Collections;
using UnityEngine;

public class EnemyRanged : EnemyController
{
    [Header("Ranged Enemy Variables")]
    public GameObject bulletPrefab;
    public Transform bulletFirePoint;
    public float shootForce;
    public float spreadAngle;

    private void Start()
    {
        enemyId = 2;
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

    #region Patrol
    public override void Patrol()
    {
        base.Patrol();
    }
    #endregion

    #region Alert
    public override void Alert()
    {
        mainAuraParticle.SetActive(true);
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
        Vector3 baseShootDirection = (Player.transform.position - bulletFirePoint.position).normalized;
        Vector3 spreadDirection = ApplySpread(baseShootDirection);

        GameObject bulletProjectile = Instantiate(bulletPrefab, bulletFirePoint.position, bulletFirePoint.rotation);
        bulletProjectile.GetComponent<Rigidbody>().AddForce(spreadDirection * shootForce, ForceMode.Impulse);
        Animator.SetBool("Attack", true);
        RandomiseAttackAudio();
        yield return new WaitForSeconds(0.5f); //Delay before hitting player
        lastAttackTime = Time.time;
        Animator.SetBool("Attack", false);
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    private void RandomiseAttackAudio()
    {
        int soundIndex = Random.Range(1, 3);
        string soundName = $"Enemy Ranged {soundIndex}";
        AudioManager.Instance.PlayOneShot(soundName, gameObject);
    }

    private Vector3 ApplySpread(Vector3 baseDirection)
    {
        float randomSpreadX = Random.Range(-spreadAngle, spreadAngle);
        float randomSpreadY = Random.Range(-spreadAngle, spreadAngle);

        Quaternion spreadRotation = Quaternion.Euler(randomSpreadY, randomSpreadX, 0);
        return spreadRotation * baseDirection;
    }
    #endregion

    #region Taking Damage
    public override void TakeDamage(float damageTaken)
    {
        StartCoroutine(DamageAura());
        base.TakeDamage(damageTaken);
    }

    private IEnumerator DamageAura()
    {
        damagedParticle.SetActive(true);
        yield return new WaitForSeconds(1f);
        damagedParticle.SetActive(false);
    }
    #endregion

    #region Death
    public override void Dead()
    {
        base.Dead();
    }
    #endregion
}
