using TMPro;
using UnityEngine;

public class TutorialPopup : MonoBehaviour
{
    [Header("Popup References")]
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI bodyText;
    [SerializeField] RectTransform popupRect;

    public void SetupPopup(string title, string body, Vector2 anchoredPos)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
        if (bodyText != null)
        {
            bodyText.text = body;
        }
        if (popupRect != null)
        {
            popupRect.anchoredPosition = anchoredPos;
        }
    }

    public void DestroyPopup()
    {
        Destroy(gameObject);
    }
}
