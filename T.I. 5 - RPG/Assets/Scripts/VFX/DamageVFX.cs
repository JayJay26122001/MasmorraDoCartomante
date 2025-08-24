using UnityEngine;
using TMPro;
public class DamageVFX : MonoBehaviour
{
    TextMeshPro text;
    float t;
    public float speed;
    bool moving = false;
    Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
        text = GetComponent<TextMeshPro>();
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        text.text = "";
        //SetDamage(99);
    }

    private void Update()
    {
        if (moving)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
        }
    }

    public void SetDamage(int damage)
    {
        transform.position = startPos;
        t = 0;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        text.text = damage.ToString();
        moving = true;
        Appear();
    }

    void Appear()
    {
        if(t < 1)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, t);
            t = Mathf.Clamp(t + 0.01f, 0, 1);
            Invoke("Appear", 0.0001f);
        }
        else
        {
            Invoke("Disappear", 1);
        }
    }
    void Disappear()
    {
        if(t > 0)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, t);
            t = Mathf.Clamp(t - 0.01f, 0, 1);
            Invoke("Disappear", 0.0001f);
        }
    }
}
