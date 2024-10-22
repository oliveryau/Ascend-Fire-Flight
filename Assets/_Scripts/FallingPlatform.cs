using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float destroyDelay;
    public float fallSpeed;
    public bool isFalling;

    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public Rigidbody Rb;
    private Coroutine fallingCoroutine;
    private Animator Animator;

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();

        Rb.isKinematic = true;
        initialPosition = transform.position;
    }

    public void StartShaking(GameObject trigger, float fallDelay, FallingPlatform platform = null, float shakeDelay = 0f)
    {
        if (!isFalling) 
        {
            isFalling = true;
            fallingCoroutine = StartCoroutine(FallingSequence(trigger, fallDelay, platform, shakeDelay));
        }
    }

    private IEnumerator FallingSequence(GameObject trigger, float fallDelay, FallingPlatform platform = null, float shakeDelay = 0f)
    {
        yield return new WaitForSeconds(shakeDelay);

        Animator.SetBool("Shake", true);
        AudioManager.Instance.Play("Falling Platform Shake", gameObject);

        yield return new WaitForSeconds(fallDelay);

        Rb.isKinematic = false;
        Rb.velocity = new Vector3(0, -fallSpeed, 0);

        AudioManager.Instance.Stop("Falling Platform Shake", gameObject);
        AudioManager.Instance.PlayOneShot("Falling Platform Break", gameObject);
        Animator.SetBool("Shake", false);

        yield return new WaitForSeconds(destroyDelay);

        if (trigger != null)
        {
            var triggerComponent = trigger.GetComponent<FallingPlatformTrigger>();
            var waveComponent = trigger.GetComponent<EnemyBossPlatformWave>();

            if (triggerComponent != null) triggerComponent.CheckPlatform();
            else if (waveComponent != null) waveComponent.CheckPlatform(platform);
        }

        isFalling = false;
        fallingCoroutine = null;
    }

    public void Reset()
    {
        if (fallingCoroutine != null)
        {
            StopCoroutine(fallingCoroutine);
            fallingCoroutine = null;
        }

        transform.position = initialPosition;
        Rb.isKinematic = true;
        isFalling = false;
        Animator.SetBool("Shake", false);
        AudioManager.Instance.Stop("Falling Platform Shake", gameObject);
    }
}
