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
                CardDisplay aux = CardUIController.instance.InstantiateCard(c);
                aux.pack = this;
                cardsInstances.Add(aux.cardData);
                CardUIController.OrganizeBoughtPackCards(this);
            }
        }
        else
        {
            Debug.Log("You don't have enough money.");
        }
    }

    public void DestroyBoughtCards()
    {
        foreach(Card c in cardsInstances)
        {
            Destroy(c.cardDisplay.gameObject);
        }
    }
}
