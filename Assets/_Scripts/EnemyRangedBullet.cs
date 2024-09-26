using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyRangedBullet : MonoBehaviour
{
    public int damage;
    public float lifeTime;

    private void Start()
    {
        damage = FindFirstObjectByType<EnemyRanged>().attackDamage;
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
