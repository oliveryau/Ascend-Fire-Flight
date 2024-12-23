using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ProjectileLeftExplosion : MonoBehaviour
{
    public float explosionRadius;

    private UiManager UiManager;
    private float damage;

    private void Start()
    {
        UiManager = FindFirstObjectByType<UiManager>();
        damage = FindFirstObjectByType<PlayerController>().leftProjectileDamage;
        ApplyExplosionDamage();
        Destroy(gameObject, 2f);
        
        RandomiseAudio();
    }

    private void ApplyExplosionDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        bool hitOnce = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy")) //Normal damage melee
            {
                hitCollider.GetComponent<EnemyController>().TakeDamage(damage);
                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
            }
            else if (hitCollider.CompareTag("Enemy Boss"))
            {
                hitCollider.GetComponent<EnemyBoss>().TakeDamage(damage); 
                hitCollider.GetComponent<EnemyBoss>().FlashHealthBar("Fire"); //Ui animate fire
                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
            }
            else if (hitCollider.CompareTag("Enemy Spawner")) //Normal damage spawner
            {
                if (hitCollider.GetComponent<EnemyBossMeleeSpawner>() != null) hitCollider.GetComponent<EnemyBossMeleeSpawner>().TakeDamage(damage); //Boss spawner script hp
                else if (hitCollider.GetComponent<EnemySpawner>() != null) hitCollider.GetComponent<EnemySpawner>().TakeDamage(damage); //Spawner script hp
                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
            }
            else if (hitCollider.CompareTag("Enemy Ranged")) //Zero damage ranged
            {
                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
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
