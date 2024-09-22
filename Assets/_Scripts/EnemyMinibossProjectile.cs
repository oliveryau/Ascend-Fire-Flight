using UnityEngine;

public class EnemyMinibossProjectile : MonoBehaviour
{
    public int damage;

    private void Start()
    {
        damage = FindFirstObjectByType<EnemyMiniboss>().attackDamage;
        Destroy(gameObject, 5f);
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Player"))
        {
            target.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
            //on contact point, spawn particle blast
            Destroy(gameObject);
        }
    }
}
