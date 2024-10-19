using UnityEngine;

public class HealingPond : MonoBehaviour
{
    private Animator Animator;
    private SphereCollider DetectionRadius;

    private void Start()
    {
        Animator = GetComponent<Animator>();
        DetectionRadius = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            Animator.SetTrigger("Light Up");
        }
    }

    private void OnTriggerExit(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            Animator.SetTrigger("Light Down");
        }
    }
}
