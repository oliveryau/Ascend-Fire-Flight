using System.Collections;
using UnityEngine;

public class FallingPlatformTrigger : MonoBehaviour
{
    public GameObject fallingPlatform;
    public float respawnDelay;

    private bool isFalling;
    private Vector3 initialPosition;

    private void Start()
    {
        respawnDelay = fallingPlatform.GetComponent<FallingPlatform>().destroyDelay + 2f;
        initialPosition = fallingPlatform.transform.position;
    }

    private IEnumerator Respawning()
    {
        yield return new WaitForSeconds(respawnDelay);
        fallingPlatform.transform.position = initialPosition;
        fallingPlatform.GetComponent<Rigidbody>().isKinematic = true;
        fallingPlatform.SetActive(true);
        isFalling = false;
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player") && !isFalling)
        {
            isFalling = true;
            StartCoroutine(fallingPlatform.GetComponent<FallingPlatform>().Falling());
            StartCoroutine(Respawning());
        }
    }
}
