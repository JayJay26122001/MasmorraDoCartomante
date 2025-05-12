using UnityEngine;
using System.Collections.Generic;

public class CardPack : MonoBehaviour
{
    public CardPool possibleCards;
    public List<Card> cards;
    public List<Card> cardsInstances;
    public int price, cardQuantity, selectableCardsQuantity;

    private void Start()
    {
        DefineCards();
        foreach (Card c in cards)
        {
            Debug.Log(c.Name);
        }
    }

    public void DefineCards()
    {
        cards = possibleCards.SelectCards(cardQuantity);
    }

    public void OnMouseDown()
    {
        if (GameplayManager.instance.ChangeMoney(-price))
        {
            Debug.Log("Pack bought.");
            foreach(Card c in cards)
            {
                Card aux = CardUIController.instance.InstantiateCard(c).cardData;
                cardsInstances.Add(aux);
                CardUIController.OrganizeBoughtPackCards(this);
            }
        }
        else
        {
            Debug.Log("You don't have enough money.");
        }
    }
}
