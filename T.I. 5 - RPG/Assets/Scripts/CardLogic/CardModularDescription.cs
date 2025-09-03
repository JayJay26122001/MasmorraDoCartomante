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

        //currentDamage = cardData.CardDamage();
    }

    void Update()
    {
        UpdateCardEffectDescription();
        //UpdateValue();
    }

    public void UpdateCardEffectDescription()
    {
        string desc = cardData.Description;
        if(cardData.instantaneous && cardData.limited)
        {
            desc += " INSTANTANEOUS & LIMITED";
        }
        else if(cardData.instantaneous && !cardData.limited)
        {
            desc += " INSTANTANEOUS";
        }
        else if (!cardData.instantaneous && cardData.limited)
        {
            desc += " LIMITED";
        }
        descriptionText.text = desc;
    }

    /*public void UpdateValue()
    {
        string desc = descriptionText.text;
        if (currentDamage == cardData.CardDamage())
        {
            desc = desc.Replace("v{Damage}", currentDamage.ToString());
        }
        else
        {
            desc = desc.Replace("v{Damage}", $"<color=#FF5555>{currentDamage}</color>");
        }
        descriptionText.text = desc;
    }*/

}
