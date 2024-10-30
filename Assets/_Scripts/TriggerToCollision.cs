using UnityEngine;

public class TriggerToCollision : MonoBehaviour
{
    public GameObject block;

    private void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("Player"))
        {
            block.GetComponent<BoxCollider>().isTrigger = false;
        }
    }
}
