using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

public class Player : Creature
{
    public List<Card> SelectedCards;
    int totalEnergyCost = 0;
    public void PlaySelectedCards()
    {
        foreach (Card c in SelectedCards)
        {
            PlayCard(c);
        }
        totalEnergyCost = 0;
    }
    public void SelectCard(Card c)
    {
        if (c.cost + totalEnergyCost <= energy)
        {
            totalEnergyCost += c.cost;
            SelectedCards.Add(c);
        }
    }
    public void DiselectCard(Card c)
    {
        if (SelectedCards.Contains(c))
        {
            totalEnergyCost -= c.cost;
            Mathf.Clamp(totalEnergyCost, 0, math.INFINITY);
            SelectedCards.Remove(c);
        }
        
    }
}

