using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    public Checkpoint checkPoint;

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            PlayerController player = target.GetComponent<PlayerController>();
            checkPoint.SetNewCheckpoint(player);
        }
    }
}
