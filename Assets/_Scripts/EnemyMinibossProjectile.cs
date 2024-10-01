using UnityEngine;

public class EnemyMinibossProjectile : MonoBehaviour
{
    public int damage;
    public float lifeTime;

    private void Start()
    {
        damage = FindFirstObjectByType<EnemyMiniboss>().attackDamage;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Player"))
        {
            target.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
