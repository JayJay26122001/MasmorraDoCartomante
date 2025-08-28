using UnityEngine;

public class DisappearingObject : MonoBehaviour
{
    public Material mat;
    public GameObject extraObj;
    float animTimeStart;
    bool inAnimation, disappearing;
    float t;

    private void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        mat.SetFloat("_DisappearTime", 1);
        if (extraObj != null)
        {
            extraObj.SetActive(false);
        }
    }
    public void AnimateObject(bool disappear)
    {
        disappearing = disappear;
        if(extraObj != null && disappear)
        {
            extraObj.SetActive(false);
        }
        animTimeStart = Time.time;
        inAnimation = true;
    }

    private void Update()
    {
        if (inAnimation)
        {
            if (disappearing)
            {
                t = Mathf.Clamp((Time.time - animTimeStart) * 3, 0, 1);
            }
            else
            {
                t = Mathf.Clamp(1 - ((Time.time - animTimeStart) * 3), 0, 1);
            }
            mat.SetFloat("_DisappearTime", t);
            if ((t >= 1 && disappearing) || (t <= 0 && !disappearing))
            {
                inAnimation = false;
                if (!disappearing)
                {
                    if(extraObj != null)
                    {
                        extraObj.SetActive(true);
                    }
                }
            }
        }
    }
}
