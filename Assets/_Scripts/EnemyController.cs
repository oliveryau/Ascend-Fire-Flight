using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { WAIT, PATROL, ALERT, ATTACK, FLEE}
    public EnemyState currentEnemyState;

    [Header("Enemy Base Variables")]
    public float maxHealth;
    public float currentHealth;
    public float moveSpeed;

    [Header("Radii")]
    public float detectionRadius;
    public float attackRadius;

    [Header("Attack")]
    public float attackDamage;
    public float attackCooldown;

    private void Start()
    {
        currentEnemyState = EnemyState.PATROL;
    }

    private void Update()
    {
        CheckEnemyState();
    }

    private void CheckEnemyState()
    {
        switch (currentEnemyState)
        {
            case EnemyState.WAIT:
                break;
            case EnemyState.PATROL:
                //Patrol();
            case EnemyState.ALERT:
                //Alert(); //Move towards player
            case EnemyState.ATTACK:
                //Attack();
            case EnemyState.FLEE:
                //Flee(); //Move back to patrol
                break;
        }
    }


}
