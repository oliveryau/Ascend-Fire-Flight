using UnityEngine;

public class EnemyRangedProjectile : MonoBehaviour
{
    public float lifeTime;
    public GameObject hitVfx;

    private float damage;

    private void Start()
    {
        Physics.IgnoreLayerCollision(6, 8);
        damage = FindFirstObjectByType<EnemyRanged>().attackDamage;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Player"))
        {
            target.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Ground") || target.gameObject.CompareTag("Lava"))
        {
            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
    }
}
