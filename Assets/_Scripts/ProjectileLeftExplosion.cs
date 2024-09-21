using UnityEngine;

public class ProjectileLeftExplosion : MonoBehaviour
{
    public int damage;
    public float explosionRadius;

    private void Start()
    {
        damage = FindFirstObjectByType<PlayerController>().rightProjectileDamage;
        ApplyExplosionDamage();
        Destroy(gameObject, 2f); //Remove when particle can destroy itself
    }

    private void ApplyExplosionDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                hitCollider.GetComponent<EnemyController>().TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
