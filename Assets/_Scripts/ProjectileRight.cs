using UnityEngine;

public class ProjectileRight : MonoBehaviour
{
    public int damage;
    public GameObject hitEnemyVfx;
    public GameObject hitGroundVfx;

    private PlayerController Player;

    private void Start()
    {
        Player = FindFirstObjectByType<PlayerController>();
        damage = Player.rightProjectileDamage;

        Destroy(gameObject, 2f);
    }
    
    private void ChargeHeal()
    {
        if (Player.currentHealCharge >= Player.maxHealCharge) return;

        Player.currentHealCharge++;
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Enemy"))
        {
            target.gameObject.GetComponent<EnemyController>().TakeDamage(damage);
            ChargeHeal();

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
