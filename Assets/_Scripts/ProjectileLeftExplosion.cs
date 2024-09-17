using UnityEngine;

public class ProjectileLeftExplosion : MonoBehaviour
{
    public float damage;
    public float explosionRadius;

    private void Start()
    {
        damage = FindFirstObjectByType<PlayerController>().rightProjectileDamage;
        ApplyExplosionDamage();
        Destroy(gameObject, 2f); // Adjust the time as needed
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