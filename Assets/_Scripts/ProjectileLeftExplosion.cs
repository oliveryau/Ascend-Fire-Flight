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

        EnemyController bossController = null; //Track if boss is hit
        float highestDamage = 0f;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy Boss") || hitCollider.CompareTag("Enemy Boss Weakpoint"))
            {
                EnemyController controller = hitCollider.GetComponent<EnemyController>(); //Store the boss controller reference
                bossController = controller;

                float currentDamage = hitCollider.CompareTag("Enemy Boss Weakpoint") ? damage * 2f : damage; //Check if weak point is hit and calculate damage

                highestDamage = Mathf.Max(highestDamage, currentDamage); //Track highest damage to apply

                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }

                continue; //Wait until all colliders checked
            }

            //Handle other enemy types normally
            if (hitCollider.CompareTag("Enemy"))
            {
                hitCollider.GetComponent<EnemyController>().TakeDamage(damage * 2f); //Double damage melee
                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
            }
            else if (hitCollider.CompareTag("Enemy Ranged"))
            {
                //hitCollider.GetComponent<EnemyController>().TakeDamage(damage * 0.5f); //Zero damage ranged
                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
            }
            else if (hitCollider.CompareTag("Enemy Spawner"))
            {
                hitCollider.GetComponent<EnemyBossMeleeSpawner>().TakeDamage(damage); //Normal damage spawner
                if (!hitOnce)
                {
                    UiManager.UpdateLeftCrosshair("Hit");
                    hitOnce = true;
                }
            }
        }

        if (bossController != null && highestDamage > 0)
        {
            bossController.TakeDamage(highestDamage); //Apply the highest damage to the boss if hit
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
