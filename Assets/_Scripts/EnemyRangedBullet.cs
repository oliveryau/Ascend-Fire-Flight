using UnityEngine;

public class EnemyRangedBullet : MonoBehaviour
{
    public float damage;

    private void Start()
    {
        damage = FindFirstObjectByType<EnemyRanged>().attackDamage;
        Destroy(gameObject, 5f);
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