using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float destroyDelay;
    public float fallSpeed;

    private Rigidbody Rb;
    private Animator Animator;

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();

        Rb.isKinematic = true; //No physics
    }

    public IEnumerator Falling(FallingPlatformTrigger trigger, float fallDelay)
    {
        Animator.SetBool("Shake", true);
        AudioManager.Instance.Play("Falling Platform Shake", gameObject);
        yield return new WaitForSeconds(fallDelay);
        Rb.isKinematic = false;
        Rb.velocity = new Vector3(0, -fallSpeed, 0);
        AudioManager.Instance.Stop("Falling Platform Shake", gameObject);
        AudioManager.Instance.PlayOneShot("Falling Platform Break", gameObject);
        Animator.SetBool("Shake", false);
        yield return new WaitForSeconds(destroyDelay);
        trigger.DestroyTrigger();
        Destroy(gameObject);
    }
}
