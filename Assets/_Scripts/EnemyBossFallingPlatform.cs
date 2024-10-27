using System.Collections;
using UnityEngine;

public class EnemyBossFallingPlatform : MonoBehaviour
{
    [Header("Platform Variables")]
    public float shakeDelay;
    public float fallDelay;
    public float destroyDelay;
    public float fallSpeed;

    public bool isFalling;

    private Vector3 initialPosition;
    private Rigidbody Rb;
    private Coroutine fallingCoroutine;
    private Animator Animator;

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();

        Rb.isKinematic = true;
        initialPosition = transform.position;
    }

    public void StartShaking(EnemyBossFallingPlatformTrigger trigger)
    {
        if (!isFalling)
        {
            fallingCoroutine = StartCoroutine(FallingSequence(trigger));
        }
    }

    private IEnumerator FallingSequence(EnemyBossFallingPlatformTrigger trigger)
    {
        isFalling = true;
        yield return new WaitForSeconds(shakeDelay);

        Animator.SetBool("Shake", true);
        AudioManager.Instance.Play("Falling Platform Shake", gameObject);

        yield return new WaitForSeconds(fallDelay);

        Rb.isKinematic = false;
        Rb.constraints = RigidbodyConstraints.None;
        Rb.constraints = RigidbodyConstraints.FreezeRotation;
        Rb.velocity = new Vector3(0, -fallSpeed, 0);
        Animator.SetBool("Shake", false);
        AudioManager.Instance.Stop("Falling Platform Shake", gameObject);
        AudioManager.Instance.PlayOneShot("Falling Platform Break", gameObject);

        yield return new WaitForSeconds(destroyDelay);

        trigger.RespawnPlatform();
    }

    public void Reset()
    {
        if (fallingCoroutine != null)
        {
            StopCoroutine(fallingCoroutine);
            fallingCoroutine = null;
        }

        isFalling = false;
        transform.position = initialPosition;
        Rb.isKinematic = true;
    }
}
