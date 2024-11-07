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
        if (target.gameObject.CompareTag("Enemy") || target.gameObject.CompareTag("Enemy Boss") || 
            target.gameObject.CompareTag("Enemy Spawner")) //Zero damage, update crosshair
        {
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitNoDamageVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Enemy Boss Weakpoint")) //Normal damage weakpoint, update crosshair
        {
            target.gameObject.GetComponentInParent<EnemyController>().TakeDamage(damage); //Get parent component
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitEnemyVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Enemy Ranged")) //Normal damage ranged, update crosshair
        {
            target.gameObject.GetComponent<EnemyController>().TakeDamage(damage);
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitEnemyVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Healing") || target.gameObject.CompareTag("Rubble Ice")) //Zero damage, no dmg vfx
        {
            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitNoDamageVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Rubble Fire")) //Zero damage, hitground vfx
        {
            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitGroundVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Ground") || target.gameObject.CompareTag("Lava"))
        {
            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitGroundVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
    }
}
