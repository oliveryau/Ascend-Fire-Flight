using System.Collections;
using UnityEngine;

public class FallingPlatformTrigger : MonoBehaviour
{
    public FallingPlatform fallingPlatform;
    public float fallDelay;
    public float respawnDelay;

    private bool isFalling;
    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = fallingPlatform.gameObject.transform.position;
    }

    public void CheckPlatform()
    {
        if (respawnDelay <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(RespawnPlatform());
        }
    }

    public IEnumerator RespawnPlatform()
    {
        fallingPlatform.gameObject.SetActive(false);
        yield return new WaitForSeconds(respawnDelay);
        fallingPlatform.transform.position = initialPosition;
        fallingPlatform.GetComponent<Rigidbody>().isKinematic = true;
        fallingPlatform.gameObject.SetActive(true);
        isFalling = false;
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player") && !isFalling)
        {
            isFalling = true;
            StartCoroutine(fallingPlatform.Falling(this, fallDelay));
        }
    }
}
