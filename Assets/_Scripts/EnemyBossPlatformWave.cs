using System.Collections;
using UnityEngine;

public class EnemyBossPlatformWave : MonoBehaviour
{
    public float fallDelay;
    public float respawnDelay;
    public float timeBetweenWaves;
    public bool isFalling;

    private FallingPlatform[] fallingPlatforms;
    public Vector3[] initialPosition;

    private void Start()
    {
        fallingPlatforms = GetComponentsInChildren<FallingPlatform>();

        initialPosition = new Vector3[fallingPlatforms.Length];
        for (int i = 0; i < fallingPlatforms.Length; ++i)
        {
            initialPosition[i] = fallingPlatforms[i].gameObject.transform.position;
        }
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

    public IEnumerator RespawnPlatform(FallingPlatform platform)
    {
        platform.gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);

        int index = System.Array.IndexOf(fallingPlatforms, platform);
        platform.transform.position = initialPosition[index];
        platform.GetComponent<Rigidbody>().isKinematic = true;
        platform.gameObject.SetActive(true);
        isFalling = false;
    }
}
