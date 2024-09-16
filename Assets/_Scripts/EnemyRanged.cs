using UnityEngine;

public class EnemyRanged : EnemyController
{
    private void Start()
    {
        enemyId = 2;
        InitializeEnemy();
    }

    private void Update()
    {
        CheckEnemyState();
    }

    public override void Patrol()
    {
        base.Patrol();
    }

    public override void Alert()
    {
        base.Alert();
    }

    public override void Attack()
    {
        base.Attack();
    }

    public override void Dead()
    {
        base.Dead();
    }
}
