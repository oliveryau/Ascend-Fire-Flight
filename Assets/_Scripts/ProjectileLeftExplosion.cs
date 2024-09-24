using UnityEngine;

public class ProjectileLeftExplosion : MonoBehaviour
{
    public int damage;
    public float explosionRadius;

    private void Start()
    {
        damage = FindFirstObjectByType<PlayerController>().rightProjectileDamage;
        ApplyExplosionDamage();
        Destroy(gameObject, 2f);
        
        RandomiseAudio();
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

    private void RandomiseAudio()
    {
        int number = Random.Range(0, 2);
        if (number == 0) AudioManager.Instance.PlayOneShot("Left Explosion 1", gameObject);
        else if (number == 1) AudioManager.Instance.PlayOneShot("Left Explosion 2", gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
