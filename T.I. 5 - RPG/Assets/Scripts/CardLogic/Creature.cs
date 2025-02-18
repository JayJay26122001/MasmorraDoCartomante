using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
    private void OnEnable() // Metodo de teste remover depois
    {
        SetupStartingDecks();
        decks[0].AddCard(decks[1].cards[1]);
    }

    [SerializeField] List<Deck> DeckPresets = new List<Deck>();
    public List<Deck> decks = new List<Deck>();
    public List<Card> hand = new List<Card>();
    public List<Card> playedCards = new List<Card>();
    public int hp;

    public void SetupStartingDecks()
    {
        foreach (Deck deck in DeckPresets)
        {
            AddDeck(deck);
        }
    }
    public void AddDeck(Deck preset)
    {
        Deck deck = Instantiate(preset);
        decks.Add(deck);
        deck.Setup();
    }
    public void BuyCards(int quantity)
    {
        for (; quantity > 0; quantity--)
        {
            if (decks[0].BuyingPile.Count == 0)
            {
                decks[0].ShuffleDeck();
            }
            hand.Add(decks[0].BuyingPile.Pop());
        }
    }
    public void BuyCards(int quantity, int deck)
    {
        for (; quantity > 0; quantity--)
        {
            if (decks[deck].BuyingPile.Count == 0)
            {
                decks[deck].ShuffleDeck();
            }
            hand.Add(decks[deck].BuyingPile.Pop());
        }
    }
    public void PlayCard(Card c)
    {
        hand.Remove(c);
        playedCards.Add(c);
    }
    public void DiscardCard(Card card)
    {
        card.deck.DiscardPile.Push(card);
    }
    public void EndCombat()
    {
        foreach (Deck deck in decks)
        {
            hand.Clear();
            playedCards.Clear();
            deck.ResetPiles();
        }
    }
}
