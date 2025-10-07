using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "CardLogic/Deck")]
public class Deck : ScriptableObject
{
    public Creature Owner;
    [SerializeField] List<Card> CardPresets = new List<Card>();
    public List<Card> cards = new List<Card>();
    [NonSerialized] public List<Card> allCards = new List<Card>();
    public SerializableStack<Card> BuyingPile = new SerializableStack<Card>(), DiscardPile = new SerializableStack<Card>();
    public void Setup()
    {
        foreach (Card c in CardPresets)
        {
            AddCard(c);
        }
    }

    public Card AddCard(Card preset)
    {
        if (Owner is Player && !GameManager.instance.UnlockedCards.Contains(preset))
        {
            GameManager.instance.UnlockCard(preset);
        }
        Card card = Instantiate(preset);
        card.Setup();
        card.deck = this;
        cards.Add(card);
        allCards.Add(card);
        CardUIController.instance.InstantiateCard(card);
        return card;
    }
    public Card AddTemporaryCard(Card preset)
    {
        Card card = Instantiate(preset);
        card.Temporary = true;
        card.Setup();
        card.deck = this;
        allCards.Add(card);
        Owner.hand.Add(card);
        CardUIController.instance.InstantiateCard(card);
        CardUIController.AttCardDescription(Owner);
        CardUIController.OrganizeHandCards(Owner);
        return card;
    }
    public Card AddTemporaryCard(Card preset, CreateCard.Pile selectedPile)
    {
        Card card = Instantiate(preset);
        card.Temporary = true;
        card.Setup();
        card.deck = this;
        allCards.Add(card);
        switch (selectedPile)
        {
            case CreateCard.Pile.Hand:
                Owner.hand.Add(card);
                break;
            case CreateCard.Pile.BuyingPile:
                BuyingPile.Add(card);
                break;
            case CreateCard.Pile.DiscardPile:
                DiscardPile.Add(card);
                break;
            default:
                Owner.hand.Add(card);
                break;
        }

        CardUIController.instance.InstantiateCard(card);
        CardUIController.AttCardDescription(Owner);
        CardUIController.CardsOrganizer(Owner);
        return card;
    }

    public void RemoveCard(CardDisplay c)
    {
        Destroy(c.gameObject);
        cards.Remove(c.cardData);
        allCards.Remove(c.cardData);
        /*for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == c.cardData)
            {
                Destroy(cards[i].cardDisplay.gameObject);
                cards.RemoveAt(i);
                i = cards.Count;
            }
        }*/
    }
    public void RemoveTemporaryCards()
    {
        List<Card> aux = allCards.ToList();
        foreach (Card c in aux)
        {
            if (c.Temporary)
            {
                RemoveCard(c.cardDisplay);
            }
        }
    }
    public void ShuffleDeck() // coloca todas as cartas na pilha de descarte na pilha de compras e as embaralha
    {
        while (DiscardPile.Count > 0)
        {
            BuyingPile.Add(DiscardPile.GetTop());
        }
        ShufflePile(ref BuyingPile);
        CardUIController.OrganizeStack(BuyingPile, Owner.combatSpace.buyingPileSpace);
    }
    public void ShufflePile(ref SerializableStack<Card> pile) // embaralha apenas uma pilha (descarte ou compra)
    {
        pile = ListUT.Shuffle(pile);
    }
    public void StartShuffle()// embaralha as cartas do deck e bota na pilha de compra (USAR APENAS NO INICIO DO COMBATE)
    {
        ResetPiles();
        BuyingPile = ListUT.ToStack(ListUT.Shuffle(cards));
    }
    public void ResetPiles()// limpa pilha de compra e descarte
    {
        BuyingPile.Clear();
        DiscardPile.Clear();
        ResetCardsPos();
    }
    void ResetCardsPos()
    {
        foreach (Card c in cards)
        {
            LeanTween.cancel(c.cardDisplay.gameObject);
            c.cardDisplay.transform.position = UnityEngine.Vector3.up * 25;
            c.cardDisplay.transform.SetParent(Owner.transform);
        }
    }
    
}
