using UnityEngine;

public class EnemyMeleeSlash : MonoBehaviour
{
    public int damage;

    private void Start()
    {
        damage = FindFirstObjectByType<EnemyMelee>().attackDamage;
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            target.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
        }
    }
}
