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

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            target.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
