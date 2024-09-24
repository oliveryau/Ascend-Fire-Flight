using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Quaternion targetRotation;

    public void SetNewCheckpoint(PlayerController player)
    {
        player.initialPosition = transform.position;
        player.initialRotation = targetRotation;
    }
}
