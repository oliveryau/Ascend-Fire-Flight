using System.Collections;
using UnityEngine;

public class FallingPlatformTrigger : MonoBehaviour
{
    public FallingPlatform fallingPlatform;
    public float respawnDelay;

    public void RespawnPlatform()
    {
        StartCoroutine(RespawningSequence());
    }

    public IEnumerator RespawningSequence()
    {
        fallingPlatform.gameObject.SetActive(false);
        fallingPlatform.Reset();

        yield return new WaitForSeconds(respawnDelay);

        fallingPlatform.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player") && !fallingPlatform.isFalling)
        {
            fallingPlatform.StartShaking(this);
        }
    }
}
