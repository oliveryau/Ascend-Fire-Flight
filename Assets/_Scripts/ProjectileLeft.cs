using UnityEngine;

public class LeftProjectile : MonoBehaviour
{
    private bool hasCollided;

    private Rigidbody Rb;
    private Animator Animator;

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        Destroy(gameObject, 3f);
    }

    private void StopProjectile()
    {
        Rb.velocity = Vector3.zero;
        Rb.angularVelocity = Vector3.zero;
        Rb.drag = 500f;
        Rb.angularDrag = 500f;
    }

    private void OnCollisionEnter(Collision target)
    {
        if (hasCollided) return;

        if (target.gameObject.CompareTag("Enemy"))
        {
            hasCollided = true;
            StopProjectile();
            //Animator.SetTrigger("Explode");
            Destroy(gameObject, 1f); //After 1 second
        }
    }
}
