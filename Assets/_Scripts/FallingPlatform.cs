using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float fallDelay;
    public float destroyDelay;
    public float fallSpeed;

    private Rigidbody Rb;

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Rb.isKinematic = true; //No physics
    }

    public IEnumerator Falling()
    {
        yield return new WaitForSeconds(fallDelay);
        Rb.isKinematic = false;
        Rb.velocity = new Vector3(0, -fallSpeed, 0);
        yield return new WaitForSeconds(destroyDelay);
        gameObject.SetActive(false);
    }
}
