using UnityEngine;

public class ProjectileRightImpactEnemy : MonoBehaviour
{
    private void Start()
    {
        RandomiseAudio();
    }

    private void RandomiseAudio()
    {
        int soundIndex = Random.Range(1, 3);
        string soundName = $"Right Hit Enemy {soundIndex}";
        AudioManager.Instance.PlayOneShot(soundName, gameObject);
    }
}
