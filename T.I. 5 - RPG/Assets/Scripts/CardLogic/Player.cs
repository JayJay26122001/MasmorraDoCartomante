using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class Player : Creature
{
    public Card SelectedCard;

    public void PlaySelectedCards()
    {
        if (SelectedCard != null)
        {
            PlayCard(SelectedCard);
        }
    }
    public void SelectCard(Card c)
    {
        if (c.cost <= energy)
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
}

