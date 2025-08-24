using UnityEngine;

public class ShieldPiece : MonoBehaviour
{
    public Rigidbody rb;
    private void Start()
    {
        Force();
    }

    public void Force()
    {
        rb.AddForce(transform.position - transform.parent.position, ForceMode.Impulse);
    }
}
