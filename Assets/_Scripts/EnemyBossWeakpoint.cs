using UnityEngine;

public class EnemyBossWeakpoint : MonoBehaviour
{
    private EnemyBoss Boss;

    private void Start()
    {
        Boss = GetComponentInParent<EnemyBoss>();
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Right Bullet"))
        {
            Boss.FlashHealthBar("Ice");
        }
    }
}
