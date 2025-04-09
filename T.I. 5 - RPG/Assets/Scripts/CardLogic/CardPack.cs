using UnityEngine;
using System.Collections.Generic;

public class CardPack : MonoBehaviour
{
    public CardPool possibleCards;
    public List<Card> cards;
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
}
