using UnityEngine;

public class ProjectileRight : MonoBehaviour
{
    public float lifeTime;
    public GameObject hitEnemyVfx;
    public GameObject hitNoDamageVfx;
    public GameObject hitGroundVfx;

    private PlayerController Player;
    private UiManager UiManager;
    private float damage;

    private void Start()
    {
        Player = FindFirstObjectByType<PlayerController>();
        UiManager = FindFirstObjectByType<UiManager>();
        damage = Player.rightProjectileDamage;

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Enemy")) //Zero damage melee
        {
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitNoDamageVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Enemy Ranged")) //Normal damage ranged
        {
            target.gameObject.GetComponent<EnemyController>().TakeDamage(damage);
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitEnemyVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Enemy Boss"))  //Normal damage boss
        {
            target.gameObject.GetComponent<EnemyController>().TakeDamage(damage);
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitEnemyVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Enemy Spawner"))  //Normal damage spawners
        {
            target.gameObject.GetComponent<EnemyBossMeleeSpawner>().TakeDamage(damage);
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitEnemyVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Main Ground") || target.gameObject.CompareTag("Ground"))
        {
            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitGroundVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
    }
}
