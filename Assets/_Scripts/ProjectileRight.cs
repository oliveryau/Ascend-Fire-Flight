using UnityEngine;

public class ProjectileRight : MonoBehaviour
{
    public int damage;
    public GameObject hitVfx;

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
            GameObject vfxInstance = Instantiate(hitVfx, hitPosition, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
