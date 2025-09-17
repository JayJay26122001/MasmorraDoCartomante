using UnityEngine;
using UnityEngine.Events;

public class CardAttack : MonoBehaviour
{
    public Transform target;
    //public float moveSpeed, turnSpeed, launchForce;
    float t;
    Vector3 aux, startPos, targetPos;
    public UnityEvent HitTarget;

    public void SetTarget(Transform t)
    {
        target = t;
    }
    public void BezierCurve()
    {
        startPos = transform.position;
        targetPos = new Vector3(target.position.x, startPos.y, target.position.z - target.gameObject.GetComponent<CapsuleCollider>().radius / 2);
        aux = new Vector3(startPos.x + (targetPos.x - startPos.x) / 2, startPos.y + Mathf.Abs((targetPos.z - startPos.z) / 2), startPos.z + (targetPos.z - startPos.z) / 2);
        t = 0;
        gameObject.SetActive(true);
        CurveMovement();
    }

    void CurveMovement()
    {
        if(t < 1)
        {
            Vector3 p1 = Vector3.Lerp(startPos, aux, t);
            Vector3 p2 = Vector3.Lerp(aux, targetPos, t);
            Vector3 p3 = Vector3.Lerp(p1, p2, t);
            transform.position = p3;
            t += 0.01f;
            Invoke("CurveMovement", 0.0001f);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform == target)
        {
            HitTarget.Invoke();
            HitTarget.RemoveAllListeners();
            GameplayManager.instance.attacksUsed.Remove(this);
            this.gameObject.SetActive(false);
            //Destroy(this.gameObject);
        }
    }

    /*public void Launch()
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
        if (moving)
        {
            Vector3 dir = targetPos - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
        }
    }*/

}
