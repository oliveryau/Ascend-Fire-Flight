using UnityEngine;

public class EnemyBossProjectile : MonoBehaviour
{
    public float lifeTime;
    
    private float damage;

    private void Start()
    {
        damage = FindFirstObjectByType<EnemyBoss>().attackDamage;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Player"))
        {
            target.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
