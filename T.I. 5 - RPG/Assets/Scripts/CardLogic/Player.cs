

using Unity.VisualScripting;
using UnityEngine;

public class Player : Creature
{
    public Card SelectedCard;

    public void PlaySelectedCard()
    {
        if(SelectedCard != null)
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
            //CardUIController.HighlightSelectedCard(this);
        }
    }
    public override void PlayCard(Card c)
    {
        if (energy < c.cost || !hand.Contains(c) || !canPlayCards)
        {
            return;
        }
        energy -= c.cost;
        hand.Remove(c);
        playedCards.Add(c);
        CardUIController.OrganizeHandCards(this);
        CardUIController.OrganizePlayedCards(this);
        SceneAnimationController.AnimController.InvokeTimer(c.CardPlayed, 0.2f);
        SceneAnimationController.AnimController.InvokeTimer(PlayedCard.Invoke, c, 0.2f);
    }

    public override void BuyCards(int quantity)
    {
        bool shuffled = false;
        float basetime = 0;
        for (int i = 0; i < quantity; i++)
        {
            if (decks[0].BuyingPile.Count == 0 || shuffled)
            {
                if (decks[0].DiscardPile.Count != 0)
                {
                    decks[0].ShuffleDeck();
                    shuffled = true;
                }
                else if (decks[0].BuyingPile.Count == 0)
                {
                    return;
                }
                /*Card arg = decks[0].BuyingPile.GetTop();
                SceneAnimationController.AnimController.InvokeTimer(hand.Add, arg, 1);
                SceneAnimationController.AnimController.InvokeTimer(CardUIController.OrganizeHandCards, this, 1);
                SceneAnimationController.AnimController.InvokeTimer(CardUIController.OrganizeStack, decks[0].BuyingPile, combatSpace.buyingPileSpace, 1);*/
                basetime = 1;
            }
            else
            {
                /*hand.Add(decks[0].BuyingPile.GetTop());
                CardUIController.OrganizeHandCards(this);
                CardUIController.OrganizeStack(decks[0].BuyingPile, combatSpace.buyingPileSpace);*/
                basetime = 0;
            }
            Card arg = decks[0].BuyingPile.GetTop();
            SceneAnimationController.AnimController.InvokeTimer(hand.Add, arg, basetime + i*0.2f);
            SceneAnimationController.AnimController.InvokeTimer(CardUIController.OrganizeHandCards, this, basetime + i*0.2f);
            SceneAnimationController.AnimController.InvokeTimer(CardUIController.OrganizeStack, decks[0].BuyingPile, combatSpace.buyingPileSpace, basetime + i*0.2f);
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

