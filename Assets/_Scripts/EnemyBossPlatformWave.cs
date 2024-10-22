using System.Collections;
using UnityEngine;

public class EnemyBossPlatformWave : MonoBehaviour
{
    public float fallDelay;
    public float stayDelay;
    public float respawnDelay;
    public bool hasRespawned;

    private FallingPlatform[] fallingPlatforms;
    private int respawnedPlatformCounter = 0;

    private void Start()
    {
        fallingPlatforms = GetComponentsInChildren<FallingPlatform>();
    }

    public void CheckPlatform(FallingPlatform platform)
    {
        if (respawnDelay <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(RespawnPlatform(platform));
        }
    }

    public IEnumerator RespawnPlatform(FallingPlatform platform = null)
    {
        if (hasRespawned) yield break;

        platform.gameObject.SetActive(false);
        platform.Reset();

        yield return new WaitForSeconds(respawnDelay);

        platform.gameObject.SetActive(true);

        ++respawnedPlatformCounter;
        if (respawnedPlatformCounter >= fallingPlatforms.Length)
        {
            hasRespawned = true;
        }
    }

    public void ResetWave()
    {
        hasRespawned = false;
        respawnedPlatformCounter = 0;
    }
}
