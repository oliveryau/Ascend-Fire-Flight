using UnityEngine;

public class ProjectileRightImpactGround : MonoBehaviour
{
    private void Start()
    {
        RandomiseAudio();
    }

    private void RandomiseAudio()
    {
        int soundIndex = Random.Range(1, 3);
        string soundName = $"Right Hit Ground {soundIndex}";
        AudioManager.Instance.PlayOneShot(soundName, gameObject);
    }
}
