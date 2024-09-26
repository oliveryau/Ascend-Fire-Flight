using UnityEngine;

public class Gate : MonoBehaviour
{
    public Vector3 initialPosition;
    public Vector3 targetPosition;

    [HideInInspector] public bool closedGate;

    private Vector3 currentPosition;

    private void Start()
    {
        initialPosition = transform.position;
        targetPosition = transform.position + new Vector3(0, 25f, 0);
    }

    private void Update()
    {
        if (closedGate)
        {
            CloseGate();
        }
        else
        {
            OpenGate();
        }
    }

    public void CloseGate()
    {
        if (transform.position == targetPosition) return;
        
        currentPosition = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 15f);
        transform.position = currentPosition;
    }

    public void OpenGate()
    {
        if (transform.position == initialPosition) return;

        currentPosition = Vector3.MoveTowards(transform.position, initialPosition, Time.deltaTime * 15f);
        transform.position = currentPosition;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) GetComponent<MeshCollider>().enabled = false;
    }
}
