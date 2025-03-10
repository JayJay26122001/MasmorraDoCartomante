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
        //CardSetup();
    }

    public void CardSetup()
    {
        cardCost.text = cardData.cost.ToString();
        cardName.text = cardData.Name;
        cardDescription.text = cardData.Description;
        rarity.sprite = cardsUI.cardRarity[(int)cardData.Rarity];
        type.sprite = cardsUI.cardType[(int)cardData.Type];
    }

    public void OnCardClick()
    {
        Debug.Log("Carta clicada: " + gameObject.name);
        cardData.deck.Owner.PlayCard(cardData);
    }
}
