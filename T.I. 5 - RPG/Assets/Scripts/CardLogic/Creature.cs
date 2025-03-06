using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Creature : MonoBehaviour
{
    private void Awake() // Metodo de teste remover depois
    {
        SetupStartingDecks();
        Health = maxHP;
        //decks[0].AddCard(decks[1].cards[1]);
    }
    public Creature Enemy;
    [SerializeField] List<Deck> DeckPresets = new List<Deck>();
    [SerializeField]public List<Deck> decks = new List<Deck>();
    protected List<Card> hand = new List<Card>();
    public List<Card> playedCards = new List<Card>();
    [SerializeField] int maxHP;
    [SerializeField] int hp, shld, energy, maxBaseEnergy = 3;
    public int CardBuyMax = 5;
    public UnityEvent Damaged = new UnityEvent();
    public UnityEvent<Card> PlayedCard = new UnityEvent<Card>();
    public bool canPlayCards;


    public int Health
    {
        get { return hp; }
        private set
        {
            if (value < 0) value = 0;
            if (value > maxHP) value = maxHP;
            hp = value;
        }
    }
    public int Shield
    {
        get { return shld; }
        private set
        {
            if (value < 0) value = 0;
            shld = value;
        }
    }

    //DECK MANAGMENT METHODS
    protected void SetupStartingDecks()
    {
        foreach (Deck deck in DeckPresets)
        {
            AddDeck(deck);
        }
    }
    protected void AddDeck(Deck preset)
    {
        Deck deck = Instantiate(preset);
        decks.Add(deck);
        deck.Owner = this;
        deck.Setup();
    }


    //COMBAT CARD METHODS
    public void BuyCards(int quantity)
    {
        for (; quantity > 0; quantity--)
        {
            if (decks[0].BuyingPile.Count == 0)
            {
                if(decks[0].DiscardPile.Count == 0) { return; }
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
                if(decks[deck].DiscardPile.Count == 0) { return; }
                decks[deck].ShuffleDeck();
            }
            hand.Add(decks[deck].BuyingPile.Pop());
        }
    }
    public virtual void PlayCard(Card c)
    {
        if (energy < c.cost || !hand.Contains(c) || !canPlayCards)
        {
            return;
        }
        energy -= c.cost;
        hand.Remove(c);
        playedCards.Add(c);
        c.CardPlayed();
        PlayedCard.Invoke(c);
        Debug.Log("played card");
    }
    public void TriggerPlayedCards()
    {
        List<Card> temp = playedCards.ToList();
        foreach (Card c in temp)
        {
            c.IniciateCardEffect();
        }
    }
    public void DiscardCard(Card card)
    {
        card.deck.DiscardPile.Push(card);
        hand.Remove(card);
        playedCards.Remove(card);
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


    //COMBAT METHODS
    public void TakeDamage(int damage)
    {
        if (damage < 0) damage = 0;
        int trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
        Shield -= damage;
        Health -= trueDamage;
        Damaged.Invoke();
        if (Health <= 0)
        {
            Die();
        }
    }

    public void Heal(int heal)
    {
        if (heal < 0) heal = 0;
        Health += heal;
    }

    public void AddShield(int shield)
    {
        Shield += shield;
    }
    public void resetShield()
    {
        Shield = 0;
    }
    public void resetEnergy()
    {
        energy = maxBaseEnergy;
    }

    public void Die()
    {

    }
}
