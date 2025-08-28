using UnityEngine;

public class ShieldPiece : MonoBehaviour
{
    Vector3 startPos;
    public Rigidbody rb;

    private void Awake()
    {
        startPos = transform.localPosition;
        rb.useGravity = false;
    }
    private void Start()
    {
        //Force();
    }

    public void ResetTransform()
    {
        transform.localPosition = startPos;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void DisableGravity()
    {
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    }
    public void Force()
    {
        rb.useGravity = true;
        Vector3 dir = transform.position - transform.parent.position;
        dir.y = 0;
        rb.AddForce(dir, ForceMode.Impulse);
    }
}
