using Unity.VisualScripting;
using UnityEngine;

public class ShopObject : MonoBehaviour
{
    Vector3 startPos;
    [TextArea] public string objectDescription;
    bool firstTimeMoving;
    private void Start()
    {
        firstTimeMoving = true;
    }
    public void MoveUp()
    {
        if(firstTimeMoving)
        {
            startPos = transform.localPosition;
            firstTimeMoving = false;
        }
        LeanTween.moveLocal(gameObject, startPos + Vector3.up * 0.5f, 0.25f);
    }

    public void MoveDown()
    {
        LeanTween.moveLocal(gameObject, startPos, 0.25f);
    }

    public void ResetPosition()
    {
        transform.localPosition = startPos;
    }

    string ObjectDescription()
    {
        CardPack pack = GetComponent<CardPack>();
        if(pack != null)
        {
            return pack.data.packDescription;
        }
        else
        {
            return objectDescription;
        }
    }

    public void ShowDescHUD()
    {
        GameManager.instance.uiController.ShopDescHUD(true, ObjectDescription());
    }

    public void HideDescHUD()
    {
        GameManager.instance.uiController.ShopDescHUD(false, ObjectDescription());
    }
}
