using System.Collections;
using UnityEngine;

public class FallingPlatformTrigger : MonoBehaviour
{
    public FallingPlatform fallingPlatform;
    public float fallDelay;
    public float respawnDelay;

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
        fallingPlatform.Reset();

        yield return new WaitForSeconds(respawnDelay);

        fallingPlatform.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player") && !fallingPlatform.isFalling)
        {
            fallingPlatform.StartShaking(this.gameObject, fallDelay);
        }
    }
}
