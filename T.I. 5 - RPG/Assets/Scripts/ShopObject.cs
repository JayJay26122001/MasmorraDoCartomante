using Unity.VisualScripting;
using UnityEngine;

public class ShopObject : MonoBehaviour
{
    Vector3 startPos;
    [TextArea] public string objectDescription;
    bool firstTimeMoving;
    public GameObject outline;
    public enum ObjectType { Shop, Discard, Bell, Stamp, Shredder, Fountain, StarterAttack, StarterDefense, StarterMind }
    public ObjectType type;

    private void OnEnable()
    {
        switch (type)
        {
            case ObjectType.Shop:
            case ObjectType.Discard:
            case ObjectType.Bell:
                outline.SetActive(false);
                break;
            case ObjectType.Stamp:
            case ObjectType.Shredder:
            case ObjectType.Fountain:
                outline.SetActive(true);
                break;
        }
        this.gameObject.GetComponent<Collider>().enabled = true;
    }
    private void Start()
    {
        switch (type)
        {
            case ObjectType.Shop:
                outline.SetActive(false);
                outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.2f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0.25f);
                break;
            case ObjectType.Discard:
                outline.SetActive(false);
                outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.2f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.2f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0.5f);
                break;
            case ObjectType.Bell:
                outline.SetActive(false);
                outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.yellow);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.05f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.01f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.01f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0.25f);
                break;
            case ObjectType.Stamp:
                outline.SetActive(true);
                outline.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0, 0.3f, 1, 1));
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.15f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.25f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0.1f);
                break;
            case ObjectType.Shredder:
                outline.SetActive(true);
                outline.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(1, 0.6f, 0, 1));
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.05f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0f);
                break;
            case ObjectType.Fountain:
                outline.SetActive(true);
                outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.05f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.01f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.2f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0f);
                break;
            case ObjectType.StarterAttack:
                outline.SetActive(false);
                outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.2f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0.25f);
                break;
            case ObjectType.StarterDefense:
                outline.SetActive(false);
                outline.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0, 0.3f, 1, 1));
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.2f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0.25f);
                break;
            case ObjectType.StarterMind:
                outline.SetActive(false);
                outline.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeX", 0.2f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeY", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_SizeZ", 0.1f);
                outline.GetComponent<MeshRenderer>().material.SetFloat("_Offset", 0.25f);
                break;
        }
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
