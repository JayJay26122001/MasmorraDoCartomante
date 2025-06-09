

using Unity.VisualScripting;
using UnityEngine;

public class Player : Creature
{
    public Card SelectedCard;
    public override void TurnAction()
    {
        BuyCards(5 - hand.Count);
    }

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
        if (hand.Contains(c) && c.cost <= Energy && canPlayCards)
        {
            SelectedCard = c;
            //CardUIController.HighlightSelectedCard(this);
        }
    }
    public override void PlayCard(Card c)
    {
        if (Energy < c.cost || !hand.Contains(c) || !canPlayCards)
        {
            return;
        }
        Energy -= c.cost;
        hand.Remove(c);
        playedCards.Add(c);
        CardUIController.OrganizeHandCardsWhenHighlighted(this);
        CardUIController.OrganizePlayedCards(this);
        SceneAnimationController.instance.InvokeTimer(c.CardPlayed, 0.2f);
        SceneAnimationController.instance.InvokeTimer(PlayedCard.Invoke, c, 0.2f);
    }

    public override void BuyCards(int quantity)
    {
        if (quantity <= 0) return;
        bool shuffled = false;
        float basetime = 0;
        bool boughtAll = false;
        for (int i = 0; i < quantity; i++)
        {
            if (decks[0].BuyingPile.Count == 0)
            {
                if (decks[0].DiscardPile.Count != 0)
                {
                    decks[0].ShuffleDeck();
                    shuffled = true;
                }
                else
                {
                    boughtAll = true;
                }
                basetime = 1;
            }
            else if (!shuffled)
            {
                basetime = 0;
            }
            if (!boughtAll)
            {
                Card arg = decks[0].BuyingPile.GetTop();
                SceneAnimationController.instance.InvokeTimer(hand.Add, arg, basetime + i * 0.2f);
                SceneAnimationController.instance.InvokeTimer(CardUIController.OrganizeHandCards, this, basetime + i * 0.2f);
                SceneAnimationController.instance.InvokeTimer(CardUIController.OrganizeStack, decks[0].BuyingPile, combatSpace.buyingPileSpace, basetime + i * 0.2f);
            }
        }
        GameplayManager.instance.PauseInput(basetime + quantity * 0.2f);
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

    public override void Die()
    {
        GameManager.instance.uiController.ChangeScene("GameOver");
    }
    public bool ChangeMoney(int quantity)
    {
        if (quantity < 0 && Mathf.Abs(quantity) > money)
        {
            return false;
        }
        else
        {
            money += quantity;
            GameManager.instance.uiController.UpdateMoney(money);
            return true;
        }
    }

    public void BuyHeal(int price)
    {
        if(GameplayManager.instance.canBuy)
        {
            if(ChangeMoney(-price))
            {
                ResetHP();
            }
        }
    }
}

