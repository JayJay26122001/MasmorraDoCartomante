using UnityEngine;
using System.Collections.Generic;
using TMPro;
public class DamageVFX : MonoBehaviour
{
    TextMeshPro text;
    float t;
    public float speed;
    bool goingUp;
    bool moving = false;
    //Vector3 startPos;
    public enum VFXType { Damage, PlayerHP, EnemyHP, PlayerShield, EnemyShield, PlayerEnergy, EnemyEnergy, /*Money,*/ Other };
    public List<Vector3> positions = new List<Vector3>();
    public List<Quaternion> rotations = new List<Quaternion>();
    void Start()
    {
        //startPos = transform.position;
        text = GetComponent<TextMeshPro>();
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        text.text = "";
        //SetText("CU", VFXType.Other, Color.yellow, true, -Vector3.up * 5, Quaternion.identity, 40);
    }

    private void Update()
    {
        if (moving)
        {
            if(goingUp)
            {
                transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
            }
            else
            {
                transform.Translate(-Vector3.up * speed * Time.deltaTime, Space.Self);
            }
        }
    }

    //For any VFXType except Other
    public void SetText(string tex, VFXType vfxType)
    {
        transform.position = positions[(int)vfxType];
        transform.rotation = rotations[(int)vfxType];
        switch (vfxType)
        {
            case VFXType.Damage:
                goingUp = true;
                text.color = Color.red * 0.8f;
                text.fontSize = 36;
                break;
            case VFXType.PlayerHP:
                goingUp = false;
                text.color = Color.red * 0.8f;
                text.fontSize = 20;
                break;
            case VFXType.EnemyHP:
                goingUp = false;
                text.color = Color.red * 0.8f;
                text.fontSize = 20;
                break;
            case VFXType.PlayerShield:
                goingUp = false;
                text.color = Color.blue * 0.8f + Color.green * 0.2f;
                text.fontSize = 20;
                break;
            case VFXType.EnemyShield:
                goingUp = false;
                text.color = Color.blue * 0.8f + Color.green * 0.2f;
                text.fontSize = 20;
                break;
            case VFXType.PlayerEnergy:
                goingUp = false;
                text.color = Color.green * 0.65f;
                text.fontSize = 20;
                break;
            case VFXType.EnemyEnergy:
                goingUp = false;
                text.color = Color.green * 0.65f;
                text.fontSize = 20;
                break;
            /*case VFXType.Money:
                goingUp = false;
                text.color = Color.yellow * 0.8f;
                text.fontSize = 20;
                break;*/
        }
        t = 0;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        text.text = tex;
        moving = true;
        Appear();
    }

    //Only for VFXType.Other
    public void SetText(string tex, VFXType vfxType, Color c, bool up, Vector3 pos, Quaternion rot, float size)
    {
        transform.position = pos;
        transform.rotation = rot;
        goingUp = up;
        t = 0;
        text.color = c;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        text.text = tex;
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
        else
        {
            moving = false;
            GameplayManager.instance.damageVFXUsed.Remove(this);
        }
    }
}
