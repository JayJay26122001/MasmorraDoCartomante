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
    public enum VFXType { Damage, DamageMult, PlayerHP, EnemyHP, PlayerShield, EnemyShield, PlayerEnergy, EnemyEnergy, /*Money,*/ Other };
    public List<Vector3> positions = new List<Vector3>();
    public List<Vector3> vfxPos = new List<Vector3>();
    public List<Vector3> objPos = new List<Vector3>();
    public List<Quaternion> rotations = new List<Quaternion>();
    void Start()
    {
        //startPos = transform.position;
        text = GetComponent<TextMeshPro>();
        //text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        //text.text = "";
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
            /*else
            {
                transform.Translate(-Vector3.up * speed * Time.deltaTime, Space.Self);
            }*/
        }
    }

    //For any VFXType except Other
    public void SetText(string tex, VFXType vfxType)
    {
        text = GetComponent<TextMeshPro>();
        //transform.position = positions[(int)vfxType];
        transform.position = vfxPos[(int)vfxType];
        transform.rotation = rotations[(int)vfxType];
        switch (vfxType)
        {
            case VFXType.Damage:
                goingUp = true;
                text.color = new Color(0.75f, 0.5f, 0, 1);
                //text.color = Color.red * 0.3f;
                text.alignment = TextAlignmentOptions.Right;
                text.fontSize = 36;
                break;
            case VFXType.DamageMult:
                goingUp = true;
                text.color = new Color(0.61f, 0.22f, 0.27f, 1);
                text.alignment = TextAlignmentOptions.Left;
                text.fontSize = 20;
                break;
            case VFXType.PlayerHP:
                Transform t = GameplayManager.instance.hpText.transform.parent.parent;
                transform.position = t.position + t.forward + Vector3.up * 11;
                transform.rotation = t.rotation;
                objPos[1] = t.position;
                goingUp = false;
                text.color = Color.red * 0.8f;
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 20;
                break;
            case VFXType.EnemyHP:
                goingUp = false;
                text.color = Color.red * 0.8f;
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 20;
                break;
            case VFXType.PlayerShield:
                goingUp = false;
                text.color = Color.blue * 0.8f + Color.green * 0.2f;
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 20;
                break;
            case VFXType.EnemyShield:
                goingUp = false;
                text.color = Color.blue * 0.8f + Color.green * 0.2f;
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 20;
                break;
            case VFXType.PlayerEnergy:
                goingUp = false;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Color.green * 0.65f;
                text.fontSize = 20;
                break;
            case VFXType.EnemyEnergy:
                goingUp = false;
                text.alignment = TextAlignmentOptions.Center;
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
        if (!goingUp)
        {
            Vector3 initialPos = transform.position;
            Vector3 upTarget = initialPos + Vector3.up * 1f;
            Vector3 downTarget = objPos[(int)vfxType];

            float holdStart = 0.2f;
            float holdTop = 0.1f;
            float upTime = 0.25f;
            float downTime = 0.2f;

            LeanTween.move(gameObject, initialPos, holdStart).setOnComplete(() =>
            {
                LeanTween.move(gameObject, upTarget, upTime).setEase(LeanTweenType.linear).setOnComplete(() =>
                {
                    LeanTween.delayedCall(holdTop, () =>
                    {
                        LeanTween.move(gameObject, downTarget, downTime).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
                        {
                            if(GameplayManager.currentCombat != null)
                            {
                                GameplayManager.currentCombat.CombatUI();
                            }
                            else
                            {
                                GameplayManager.instance.UpdateCreatureUI(GameplayManager.instance.player);
                            }
                        });
                    });
                });
            });
        }
    }

    //Only for VFXType.Other
    public void SetText(string tex, VFXType vfxType, Color c, bool up, Vector3 pos, Quaternion rot, float size)
    {
        text = GetComponent<TextMeshPro>();
        transform.position = pos;
        transform.rotation = rot;
        goingUp = up;
        t = 0;
        text.color = c;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        text.alignment = TextAlignmentOptions.Center;
        text.text = tex;
        moving = true;
        Appear();
    }

    public void AnimateVFX()
    {

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
