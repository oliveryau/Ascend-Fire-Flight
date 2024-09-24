using UnityEngine;

public class ProjectileRightImpactGround : MonoBehaviour
{
    private void Start()
    {
        RandomiseAudio();
    }

    private void RandomiseAudio()
    {
        int number = Random.Range(0, 2);
        if (number == 0) AudioManager.Instance.PlayOneShot("Right Hit Ground 1", gameObject);
        else if (number == 1) AudioManager.Instance.PlayOneShot("Right Hit Ground 2", gameObject);
    }
}
