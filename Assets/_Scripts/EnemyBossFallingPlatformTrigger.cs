using System.Collections;
using UnityEngine;

public class EnemyBossFallingPlatformTrigger : MonoBehaviour
{
    public EnemyBossFallingPlatform fallingPlatform;
    public float respawnDelay;

    public void RespawnPlatform()
    {
        StartCoroutine(RespawningSequence());
    }

    public IEnumerator RespawningSequence()
    {
        fallingPlatform.gameObject.SetActive(false);
        //fallingPlatform.GetComponent<MeshRenderer>().enabled = false;
        //fallingPlatform.GetComponent<MeshCollider>().enabled = false;
        fallingPlatform.Reset();

        yield return new WaitForSeconds(respawnDelay);

        fallingPlatform.gameObject.SetActive(true);
        //fallingPlatform.GetComponent<MeshRenderer>().enabled = true;
        //fallingPlatform.GetComponent<MeshCollider>().enabled = true;
    }

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player") && !fallingPlatform.isFalling)
        {
            fallingPlatform.StartShaking(this);
        }
    }
}