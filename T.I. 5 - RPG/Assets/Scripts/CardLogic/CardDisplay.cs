using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public SpriteRenderer rarity, type;
    public TextMeshPro cardCost, cardName, cardDescription;
    public Card cardData;
    public CardsUI cardsUI;

    void Start()
    {
        CardSetup();
    }

    public void CardSetup()
    {
        cardCost.text = cardData.cost.ToString();
        cardName.text = cardData.name;
        cardDescription.text = cardData.Description;
        SetSortingLayer(rarity, "CardRarity");
        SetSortingLayer(type, "CardType");
    }

    void SetSortingLayer(SpriteRenderer spriteRenderer, string layerName)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = layerName;
        }
    }
}
