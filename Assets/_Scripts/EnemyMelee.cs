using System.Collections;
using UnityEngine;

public class EnemyMelee : EnemyController
{
    [Header("Melee Enemy Variables")]
    public GameObject mainAuraParticle;
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
        Animator.SetBool("Attack", true);
        yield return new WaitForSeconds(0.5f); //Delay before hitting player
        meleeSlashParticle.SetActive(true);
        RandomiseAttackAudio();
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(0.1f);
        Animator.SetBool("Attack", false);
        meleeSlashParticle.SetActive(false);
        yield return new WaitForSeconds(attackCooldown - 0.5f);
        isAttacking = false;
    }

    private void RandomiseAttackAudio()
    {
        int soundIndex = Random.Range(1, 3);
        string soundName = $"Enemy Melee {soundIndex}";
        AudioManager.Instance.PlayOneShot(soundName, gameObject);
    }
    #endregion

    #region Death
    public override void Dead()
    {
        mainAuraParticle.SetActive(false);
        base.Dead();
    }
    #endregion
}
