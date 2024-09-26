using UnityEngine;

public class FallingPlatformTrigger : MonoBehaviour
{
    public FallingPlatform fallingPlatform;
    public float fallDelay;

    private bool isFalling;
    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = fallingPlatform.gameObject.transform.position;
    }

    public void DestroyTrigger()
    {
        Destroy(gameObject);
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
