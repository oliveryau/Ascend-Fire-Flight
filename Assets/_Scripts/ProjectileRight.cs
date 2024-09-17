using UnityEngine;

public class ProjectileRight : MonoBehaviour
{
    public float damage;

    private void Start()
    {
        damage = FindFirstObjectByType<PlayerController>().rightProjectileDamage;
        Destroy(gameObject, 2f);
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Enemy"))
        {
            target.gameObject.GetComponent<EnemyController>().TakeDamage(damage);
            //Instantiate(explosionVfx);
            Destroy(gameObject);
        }
    }
}
