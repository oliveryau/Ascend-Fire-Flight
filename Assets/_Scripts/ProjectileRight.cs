using UnityEngine;

public class ProjectileRight : MonoBehaviour
{
    public int damage;
    public float lifeTime;
    public GameObject hitEnemyVfx;
    public GameObject hitGroundVfx;

    private PlayerController Player;
    private UiManager UiManager;

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
            target.gameObject.GetComponent<EnemyController>().TakeDamage(damage);
            UiManager.UpdateRightCrosshair("Hit");

            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitEnemyVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
        else if (target.gameObject.CompareTag("Ground"))
        {
            ContactPoint contact = target.contacts[0];
            Vector3 hitPosition = contact.point;
            Destroy(Instantiate(hitGroundVfx, hitPosition, Quaternion.identity), 2f);

            Destroy(gameObject);
        }
    }
}
