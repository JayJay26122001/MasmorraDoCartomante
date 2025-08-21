using UnityEngine;

public class ShieldPiece : MonoBehaviour
{
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Force();
    }

    public void Force()
    {
        rb.AddForce(transform.position - transform.parent.position, ForceMode.Impulse);
        Destroy(transform.parent.gameObject, 3f);
    }
}
