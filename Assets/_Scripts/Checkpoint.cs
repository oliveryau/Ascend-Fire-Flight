using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public void SetNewCheckpoint(PlayerController player)
    {
        player.initialPosition = transform.position;
    }
}
