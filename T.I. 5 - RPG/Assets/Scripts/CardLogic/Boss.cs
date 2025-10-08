using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Boss : Enemy
{
    public ModularInt BonusCardsPerTurn;
    public Deck BonusDeckPrefab;
    public Deck BonusDeck;
    protected override void Awake()
    {
        base.Awake();
        InstanceBonusDeck();
    }
    /*public override void TurnAction()
    {
        if (!GameplayManager.instance.CombatActive) return;
        BuyCardAction();
        if (skipTurn > 0)
        {
            skipTurn--;
            FinishedPlaying.Invoke();
            GameplayManager.TurnArrow.NextTurn();
            return;
        }
        StartCoroutine(PlayAllCardsBehaviour());
        //StartCoroutine(PlayBonusCardsBehaviour());

    }*/
    public override void CombatStartAction()
    {
        base.CombatStartAction();
        BonusDeck.StartShuffle();
    }
    protected void InstanceBonusDeck()
    {
        Deck bonus = Instantiate(BonusDeckPrefab);
        BonusDeck = bonus;
        bonus.Owner = this;
        bonus.Setup();
        foreach (Card c in BonusDeck.cards)
        {
            c.cost = 0;
            c.cardDisplay.UpdateCardCost();
        }
    }
    protected override IEnumerator PlayAllCardsBehaviour()
    {
        yield return new WaitUntil(() => ActionController.instance.NumberOfActionsInQueue() <= 0);
        bool playanim = true;
        for (int i = 0; i < hand.Count;)
        {
            if (hand[i].cost <= energy)
            {
                Card played = hand[i];
                EnemyPlayCard anim = new EnemyPlayCard(this, played, playanim);
                ActionController.instance.AddToQueue(anim);
                playanim = false;
                //yield return new WaitUntil(() => !hand.Contains(played));
                bool CardPlayed = false;
                UnityAction<Card> check = (Card c) => CardPlayed = true;
                PlayedCard.AddListener(check);
                yield return new WaitUntil(() => CardPlayed);
                PlayedCard.RemoveListener(check);

                yield return new WaitForSeconds(0.8f);
                i = 0;
            }
            else
            {
                i++;
            }

        }
        yield return new WaitForSeconds(1f);
        StartCoroutine(PlayBonusCardsBehaviour());
    }
    IEnumerator PlayBonusCardsBehaviour()
    {
        yield return new WaitUntil(() => ActionController.instance.NumberOfActionsInQueue() <= 0);
        bool playanim = true;
        int num = BonusCardsPerTurn.GetValue();
        List<Card> bonusCards = BuyBonusCards(num);
        foreach (Card c in bonusCards)
        {
            EnemyPlayCard anim = new EnemyPlayCard(this, c, playanim);
            ActionController.instance.AddToQueue(anim);
            playanim = false;
            //yield return new WaitUntil(() => !hand.Contains(c));
            bool CardPlayed = false;
            UnityAction<Card> check = (Card c) => CardPlayed = true;
            PlayedCard.AddListener(check);
            yield return new WaitUntil(() => CardPlayed);
            PlayedCard.RemoveListener(check);

            yield return new WaitForSeconds(0.8f);
        }
        yield return new WaitForSeconds(1f);
        FinishedPlaying.Invoke();
    }
    public List<Card> BuyBonusCards(int quantity)
    {
        List<Card> cards = new List<Card>();
        for (; quantity > 0; quantity--)
        {
            if (BonusDeck.BuyingPile.Count == 0)
            {
                if (BonusDeck.DiscardPile.Count == 0)
                {
                    CardUIController.OrganizeHandCards(this);
                    CardUIController.OrganizeStack(BonusDeck.BuyingPile, combatSpace.buyingPileSpace);
                    return cards;
                }
                BonusDeck.ShuffleDeck();
            }
            Card card = BonusDeck.BuyingPile.GetTop();
            hand.Add(card);
            cards.Add(card);
        }
        CardUIController.OrganizeHandCards(this);
        CardUIController.OrganizeStack(BonusDeck.BuyingPile, combatSpace.buyingPileSpace);
        return cards;
        //CardUIController.instance.ChangePileTextValues(decks[0].BuyingPile, CardUIController.instance.buyingPilePos);   
    }
    public override void ShuffleAllDecks()
    {
        base.ShuffleAllDecks();
        BonusDeck.ShuffleDeck();
    }
}