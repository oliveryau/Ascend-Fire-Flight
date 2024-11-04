using System.Collections;
using UnityEngine;

public class RubbleFire : MonoBehaviour
{
    public bool canBeDestroyed;

    private IEnumerator DestroySequence()
    {
        canBeDestroyed = false;
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Animator>().SetTrigger("Destroy");
        AudioManager.Instance.PlayOneShot("Rubble Break", gameObject);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision target)
    {
        if (target.gameObject.CompareTag("Right Bullet") && canBeDestroyed)
        {
            StartCoroutine(DestroySequence());
        }
    }
}
