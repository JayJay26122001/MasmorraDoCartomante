using UnityEngine;

public class CardAttack : MonoBehaviour
{
    public Transform target;
    public float moveSpeed, turnSpeed, launchForce;
    float launchStart;
    bool moving = false;
    Vector3 aux, startPos, targetPos;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Launch();
    }

    public void Launch()
    {
        startPos = transform.position;
        targetPos = new Vector3 (target.position.x, startPos.y, target.position.z - target.gameObject.GetComponent<CapsuleCollider>().radius);
        //rb.AddForce(((targetPos - startPos).normalized + Vector3.up) * (targetPos - startPos).magnitude / 2f, ForceMode.Impulse);
        //Invoke("LockIn", 0.75f);
        LockIn();
    }

    public void LockIn()
    {
        launchStart = Time.time;
        moving = true;
    }

    void Update()
    {
        if(moving && Time.time - launchStart <= 1)
        {
            aux = startPos + new Vector3((Time.time - launchStart) * (targetPos.x - startPos.x), ((targetPos - startPos).magnitude/5) * Mathf.Pow((1 - Mathf.Abs(2*(Time.time - launchStart) - 1)), 0.75f), (Time.time - launchStart) * (targetPos.z - startPos.z));
            transform.Translate(aux - transform.position, Space.Self);
        }
        /*if (moving)
        {
            Vector3 dir = targetPos - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
        }*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform == target)
        {
            this.gameObject.SetActive(false);
            //Destroy(this.gameObject);
        }
    }
}
