using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float destroyDelay;
    public float fallSpeed;

    [HideInInspector] public Vector3 initialPosition;
    private Rigidbody Rb;
    private Animator Animator;

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();

        Rb.isKinematic = true; //No physics
        initialPosition = transform.position;
    }

    public IEnumerator Falling(GameObject trigger, float fallDelay, FallingPlatform platform = null)
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

        if (trigger.GetComponent<FallingPlatformTrigger>() != null)
        {
            trigger.GetComponent<FallingPlatformTrigger>().CheckPlatform();
        }
        else if (trigger.GetComponent<EnemyBossPlatformWave>() != null)
        {
            trigger.GetComponent<EnemyBossPlatformWave>().CheckPlatform(platform);
        }
    }
}
