using UnityEngine;

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
            if (hitCollider.CompareTag("Enemy")) 
            {
                hitCollider.GetComponent<EnemyController>().TakeDamage(damage * 2); //Double damage melee enemy

                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
            }
            else if (hitCollider.CompareTag("Enemy Ranged")) 
            {
                hitCollider.GetComponent<EnemyController>().TakeDamage(damage * 0.5f); //Half damage ranged enemy

                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
            }
            else if (hitCollider.CompareTag("Enemy Boss"))
            {
                hitCollider.GetComponent<EnemyController>().TakeDamage(damage); //Normal damage boss

                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
            }
            else if (hitCollider.CompareTag("Enemy Spawner"))
            {
                hitCollider.GetComponent<EnemyBossMeleeSpawner>().TakeDamage(damage); //Normal damage boss spawner

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
