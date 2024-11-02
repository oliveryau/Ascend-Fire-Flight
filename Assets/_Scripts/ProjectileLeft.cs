using System.Collections;
using UnityEngine;

public class LeftProjectile : MonoBehaviour
{
    public GameObject explosionVfx;
    public float lifeTime;

    private bool hasExploded;
    private Vector3 position;
    private Rigidbody Rb;
    private Animator Animator;

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();

        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!hasExploded)
        {
            InstantiateExplosion(transform.position);
            Destroy(gameObject);
        }
    }

    private void InstantiateExplosion(Vector3 position)
    {
        Instantiate(explosionVfx, position, Quaternion.identity);
        hasExploded = true;
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Enemy") || target.gameObject.CompareTag("Enemy Ranged") ||
            target.gameObject.CompareTag("Enemy Boss") || target.gameObject.CompareTag("Enemy Boss Weakpoint") ||
            target.gameObject.CompareTag("Enemy Spawner") || target.gameObject.CompareTag("Ground") || 
            target.gameObject.CompareTag("Lava") || target.gameObject.CompareTag("Healing") ||
            target.gameObject.CompareTag("Rubble Fire") || target.gameObject.CompareTag("Rubble Ice"))
        {
            ContactPoint contact = target.contacts[0];
            Vector3 position = contact.point;

            InstantiateExplosion(position);
            Destroy(gameObject);
        }
    }
}
