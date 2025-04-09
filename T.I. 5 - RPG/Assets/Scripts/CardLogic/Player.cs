using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class Player : Creature
{
    public Card SelectedCard;

    public void PlaySelectedCard()
    {
        if (SelectedCard != null)
        {
            Card temp = SelectedCard;
            DiselectCard();
            PlayCard(temp);
        }
    }
    public void SelectCard(Card c)
    {
        if (hand.Contains(c) && c.cost <= energy && canPlayCards)
        {
            SelectedCard = c;
        }
    }
    public void DiselectCard(Card c)
    {
        if (SelectedCard == c)
        {
            SelectedCard = null;
        }

    }
    public void DiselectCard()
    {
        SelectedCard = null;
    }
}

