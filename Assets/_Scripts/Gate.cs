using UnityEngine;

public class Gate : MonoBehaviour
{
    public Vector3 initialPosition;
    public Vector3 targetPosition;

    [HideInInspector] public bool closedGate;

    private Vector3 currentPosition;
    private bool soundPlaying;

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
        if (transform.position == targetPosition)
        {
            if (soundPlaying)
            {
                soundPlaying = false;
                AudioManager.Instance.Stop("Gate Move", gameObject);
            }
            return;
        }

        if (!soundPlaying)
        {
            soundPlaying = true;
            AudioManager.Instance.Play("Gate Move", gameObject);
        }

        currentPosition = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 10f);
        transform.position = currentPosition;

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            soundPlaying = false;
            AudioManager.Instance.Stop("Gate Move", gameObject);
            GetComponent<MeshCollider>().enabled = true;
        }
    }

    public void OpenGate()
    {
        if (transform.position == initialPosition)
        {
            if (soundPlaying)
            {
                soundPlaying = false;
                AudioManager.Instance.Stop("Gate Move", gameObject);
            }
            return;
        }

        if (!soundPlaying)
        {
            soundPlaying = true;
            AudioManager.Instance.Play("Gate Move", gameObject);
        }

        currentPosition = Vector3.MoveTowards(transform.position, initialPosition, Time.deltaTime * 10f);
        transform.position = currentPosition;

        if (Vector3.Distance(transform.position, initialPosition) < 0.01f)
        {
            transform.position = initialPosition;
            soundPlaying = false;
            AudioManager.Instance.Stop("Gate Move", gameObject);
            GetComponent<MeshCollider>().enabled = false;
        }
    }
}
