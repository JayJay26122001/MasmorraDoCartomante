using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Creature : MonoBehaviour
{
    protected virtual void Awake() // Metodo de teste remover depois
    {
        SetupStartingDecks();
        Health = maxHP;
        //decks[0].AddCard(decks[1].cards[1]);
    }
    public Creature Enemy;
    [SerializeField] List<Deck> DeckPresets = new List<Deck>();
    [SerializeField]public List<Deck> decks = new List<Deck>();
    public List<Card> hand = new List<Card>();
    public List<Card> playedCards = new List<Card>();
    [SerializeField] protected int maxHP;
    [SerializeField] protected int hp, shld, energy, maxBaseEnergy = 3, money;
    public int CardBuyMax = 5;
    [SerializeField] int baseDamage = 6, baseDefense = 5;
    public float BaseDamageMultiplier = 1, BaseDefenseMultiplier = 1;
    public UnityEvent Damaged = new UnityEvent();
    public UnityEvent<Card> PlayedCard = new UnityEvent<Card>();
    public bool canPlayCards;
    public CardCombatSpaces combatSpace;

    public TextMeshProUGUI hpText, shieldText, energyText;  //Ui das criaturas na batalha

    public int Money
    {
        get { return hp; }
    }

    public int Health
    {
        get { return hp; }
        protected set
        {
            if (value < 0) value = 0;
            if (value > maxHP) value = maxHP;
            hp = value;
        }
    }
    public int Shield
    {
        get { return shld; }
        protected set
        {
            if (value < 0) value = 0;
            shld = value;
        }
    }
    public int Energy
    {
        get { return energy; }
        protected set
        {
            if (value < 0) value = 0;
            if (value > maxBaseEnergy) value = maxBaseEnergy;
            energy = value;
        }
    }
    public int BaseDamage
    {
        get{return (int)Math.Ceiling(baseDamage * BaseDamageMultiplier);}
    }
    public int BaseDefense
    {
        get{return (int)Math.Ceiling(baseDefense * BaseDefenseMultiplier);}
    }
    public void ResetDamageMultiplier()
    {
        BaseDamageMultiplier = 1;
    }
    public void ResetDefenseMultiplier()
    {
        BaseDamageMultiplier = 1;
    }
    public void UpdateCreatureUI(Creature c)
    {
        if (hpText != null)
        {
            hpText.text = $"HP: {c.Health}";
        }
        if (shieldText != null)
        {
            shieldText.text = $"Shield: {c.Shield}";
        }
        if (energyText != null)
        {
            energyText.text = $"Energy: {c.energy}";
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

    public virtual void TurnAction() //o que essa criatura faz em seu turno
    {
        BuyCards(CardBuyMax);
    }
    public virtual void CombatStartAction()
    {
        
    }
    public virtual void BuyCards(int quantity)
    {
        for (; quantity > 0; quantity--)
        {
            if (decks[0].BuyingPile.Count == 0)
            {
                if (decks[0].DiscardPile.Count == 0) { return; }
                decks[0].ShuffleDeck();
            }
            hand.Add(decks[0].BuyingPile.GetTop());
        }
        CardUIController.OrganizeHandCards(this);
        CardUIController.OrganizeStack(decks[0].BuyingPile, combatSpace.buyingPileSpace);
    }
    public void BuyCards(int quantity, int deck)
    {
        for (; quantity > 0; quantity--)
        {
            if (decks[deck].BuyingPile.Count == 0)
            {
                if (decks[deck].DiscardPile.Count == 0) { return; }
                decks[deck].ShuffleDeck();
            }
            hand.Add(decks[deck].BuyingPile.GetTop());
        }
        CardUIController.OrganizeHandCards(this);
        CardUIController.OrganizeStack(decks[deck].BuyingPile, combatSpace.buyingPileSpace);
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
        CardUIController.OrganizeHandCards(this);
        CardUIController.OrganizePlayedCards(this);
        CardUIController.OrganizeEnemyPlayedCards(this);
        c.CardPlayed();
        PlayedCard.Invoke(c);
        //Debug.Log("played card");
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
        card.deck.DiscardPile.Add(card);
        hand.Remove(card);
        playedCards.Remove(card);
        if (card.exaust)
        {
            ExaustCard(card);
        }
        CardUIController.OrganizePlayedCards(this);
        CardUIController.OrganizeStackFlat(card.deck.DiscardPile, combatSpace.discardPileSpace);
    }
    public void ExaustCard(Card card)
    {
        hand.Remove(card);
        card.deck.DiscardPile.Remove(card);
        card.deck.BuyingPile.Remove(card);
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
    public virtual void TakeDamage(int damage, bool IgnoreDefense)
    {
        if (damage < 0) damage = 0;
        int trueDamage;
        if (IgnoreDefense)
        {
            trueDamage = damage;
        }
        else
        {
            trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
            Shield -= damage;
        }
        Health -= trueDamage;
        Damaged.Invoke();
        if (Health <= 0)
        {
            Die();
        }
    }
    public virtual void TakeDamage(int damage)
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

    public virtual void Die()
    {

    }
}
