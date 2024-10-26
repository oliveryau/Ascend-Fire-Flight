using UnityEngine;

public class ProjectileRight : MonoBehaviour
{
    public float lifeTime;
    public GameObject hitEnemyVfx;
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
        if (target.gameObject.CompareTag("Enemy"))
        {
            target.gameObject.GetComponent<EnemyController>().TakeDamage(damage * 0.5f); //Half damage melee enemy
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitEnemyVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Enemy Ranged")) 
        {
            target.gameObject.GetComponent<EnemyController>().TakeDamage(damage * 2); //Double damage ranged enemy
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitEnemyVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Enemy Boss"))
        {
            target.gameObject.GetComponent<EnemyController>().TakeDamage(damage); //Normal damage boss
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitEnemyVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Enemy Spawner"))
        {
            target.gameObject.GetComponent<EnemyBossMeleeSpawner>().TakeDamage(damage); //Normal damage boss spawners
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
