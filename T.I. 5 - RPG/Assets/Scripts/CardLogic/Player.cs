

using Unity.VisualScripting;
using UnityEngine;

public class Player : Creature
{
    public Card SelectedCard;
    public MeshFilter moneyBag;
    public Mesh[] bagMeshes = new Mesh[4];
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
        ActionController.instance.InvokeTimer(() =>
        {
            CardUIController.PlayCardVFX(CardUIController.instance.puffVfx, c.cardDisplay.transform.position);
        }, CardUIController.instance.smallTimeAnim);
        ActionController.instance.InvokeTimer(c.CardPlayed, 0.2f);
        ActionController.instance.InvokeTimer(AudioController.instance.RandomizeSfx, AudioController.instance.sfxSource, AudioController.instance.playCardSfx, 0.2f);
        ActionController.instance.InvokeTimer(PlayedCard.Invoke, c, 0.2f);
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
                    AudioController.instance.RandomizeSfx(AudioController.instance.sfxSource, AudioController.instance.shuffleDeckSfx);
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
                ActionController.instance.InvokeTimer(hand.Add, arg, basetime + i * 0.2f);
                ActionController.instance.InvokeTimer(CardUIController.OrganizeHandCards, this, basetime + i * 0.2f);
                ActionController.instance.InvokeTimer(CardUIController.OrganizeStack, decks[0].BuyingPile, combatSpace.buyingPileSpace, basetime + i * 0.2f);
                ActionController.instance.InvokeTimer(AudioController.instance.RandomizeSfx, AudioController.instance.sfxSource, AudioController.instance.receiveCardSfx, basetime + i * 0.2f);
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
            moneyBag.mesh = bagMeshes[Mathf.Clamp(money / 5, 0, 3)];
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

