using TMPro;
using UnityEngine;

public class CardModularDescription : MonoBehaviour
{
    [SerializeField] TextMeshPro descriptionText;
    CardDisplay cardDisplay;
    Card cardData;
    int currentDamage, currentDefense;

    void Start()
    {
        cardDisplay = GetComponentInParent<CardDisplay>();
        cardData = cardDisplay.cardData;
    }

    void Update()
    {
        UpdateDescription();
    }

    public void UpdateDescription()
    {
        string desc = cardDisplay.cardData.Description;
        if(cardDisplay.cardData.instantaneous)
        {
            desc += " INSTANTANEOUS";
        }
        else if(cardDisplay.cardData.limited)
        {
            desc += " LIMITED";
        }
        descriptionText.text = desc;
    }

}
