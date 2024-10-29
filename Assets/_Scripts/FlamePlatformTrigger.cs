using UnityEngine;

public class FlamePlatformTrigger : MonoBehaviour
{
    private float damage;

    private void Start()
    {
        damage = 1f;
    }

    private void OnTriggerStay(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            target.GetComponent<PlayerController>().TakeDamage(damage);
        }
    }
}
