using UnityEngine;

public class ShopObject : MonoBehaviour
{
    Vector3 startPos;

    public void MoveUp()
    {
        startPos = transform.localPosition;
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
}
