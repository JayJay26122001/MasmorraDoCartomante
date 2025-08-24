using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;

public class Creature : MonoBehaviour
{
    protected virtual void Awake() // Metodo de teste remover depois
    {
        SetupStartingDecks();
        Health = maxHP;
        //decks[0].AddCard(decks[1].cards[1]);
    }
    public Creature enemy;
    [SerializeField] List<Deck> DeckPresets = new List<Deck>();
    [SerializeField] public List<Deck> decks = new List<Deck>();
    public List<Card> hand = new List<Card>();
    public List<Card> playedCards = new List<Card>();
    [SerializeField] protected int maxHP;
    [SerializeField] protected int hp, shld, energy, maxBaseEnergy = 3, money;
    public int CardBuyMax = 5;
    [SerializeField] int baseDamage = 6, baseShieldGain = 5;
    [Range(0, 1)][SerializeField] float baseDamageReduction = 0;
    public List<StatModifier> DamageModifiers, ShieldModifiers, DamageReductionModifiers;
    //public float BaseDamageMultiplier = 1, BaseDefenseMultiplier = 1;
    public UnityEvent Damaged = new UnityEvent(), Wounded = new UnityEvent(), DamageBlocked = new UnityEvent();
    public UnityEvent<Card> PlayedCard = new UnityEvent<Card>();
    public bool canPlayCards;
    public CardCombatSpaces combatSpace;

    public TextMeshProUGUI hpText, shieldText, energyText;  //Ui das criaturas na batalha
    List<Card> exausted = new List<Card>();

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
            //if (value > maxBaseEnergy) value = maxBaseEnergy;
            energy = value;
        }
    }
    public int BaseDamage
    {
        get{ return baseDamage; }
        /*get
        {
            int res = baseDamage;
            foreach (StatModifier m in DamageModifiers)
            {
                res = m.ApplyModfier(res);
            }
            return res;
        }*/
    }
    public int BaseShieldGain
    {
        get{ return baseShieldGain; }
        /*get
        {
            int res = baseShieldGain;
            foreach (StatModifier m in ShieldModifiers)
            {
                res = m.ApplyModfier(res);
            }
            return res;
        }*/
    }
    [Range(0, 1)]
    public float BaseDamageReduction
    {
        get
        {
            float res = baseDamageReduction;
            foreach (StatModifier m in DamageReductionModifiers)
            {
                res = m.ApplyModfier(res);
            }
            return res;
        }
    }
    public void ResetDamageModifiers()
    {
        DamageModifiers.Clear();
    }
    public void ResetShieldModifiers()
    {
        ShieldModifiers.Clear();
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
            energyText.text = $"Energy: {c.Energy}";
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
        if (Energy < c.cost || !hand.Contains(c) || !canPlayCards)
        {
            return;
        }
        Energy -= c.cost;
        hand.Remove(c);
        playedCards.Add(c);
        CardUIController.OrganizeHandCards(this);
        CardUIController.OrganizePlayedCards(this);
        ActionController.instance.InvokeTimer(() =>
        {
            CardUIController.PlayCardVFX(CardUIController.instance.puffVfx, c.cardDisplay.transform.position);
            AudioController.instance.RandomizeSfx(AudioController.instance.sfxSource, AudioController.instance.playCardSfx);
        }, CardUIController.instance.mediumTimeAnim * 2 + CardUIController.instance.bigTimeAnim);
        //CardUIController.OrganizeEnemyPlayedCards(this);
        c.CardPlayed();
        PlayedCard.Invoke(c);
        //Debug.Log("played card");
    }
    public void TriggerPlayedCards()
    {
        List<Card> temp = playedCards.ToList();
        foreach (Card c in temp)
        {
            c.ApplyUnconditionalEffects();
        }
    }
    public void DiscardCard(Card card)
    {
        if (card.deck.DiscardPile.Contains(card))
        {
            return;
        }
        card.deck.DiscardPile.Add(card);
        hand.Remove(card);
        playedCards.Remove(card);
        foreach (Effect e in card.Effects)
        {
            if (!e.EffectAcomplished)
            {
                e.EffectEnded();
            }
            e.resetEffect();
        }
        if (card.limited)
        {
            ExaustCard(card);
        }
        CardUIController.OrganizePlayedCards(this);
        //if(card.deck.Owner != Player)
        CardUIController.OrganizeStackFlat(card.deck.DiscardPile, combatSpace.discardPileSpace);
    }
    public void ExaustCard(Card card)
    {
        card.cardDisplay.gameObject.SetActive(false);
        hand.Remove(card);
        card.deck.DiscardPile.Remove(card);
        card.deck.BuyingPile.Remove(card);
        playedCards.Remove(card);
        exausted.Add(card);
    }
    public void RevertExaust(Card card)
    {
        if (!card.cardDisplay.gameObject.activeSelf)
        {
            card.cardDisplay.gameObject.SetActive(true);
            card.deck.DiscardPile.Add(card);
            exausted.Remove(card);
        }
    }
    public virtual void EndCombat()
    {
        ResetDeckPiles();
        foreach (Deck deck in decks)
        {
            hand.Clear();
            //playedCards.Clear();
            deck.ResetPiles();
        }
        ResetEnergy();
        ResetShield();
        //ResetHP();
        ResetDamageModifiers();
        ResetShieldModifiers();
    }


    //COMBAT METHODS
    public virtual void TakeDamage(int damage, bool IgnoreDefense)
    {
        damage -= (int)(BaseDamageReduction * damage);
        if (damage <= 0) { return; }
        int trueDamage;
        if (IgnoreDefense)
        {
            trueDamage = damage;
            Wounded.Invoke();
        }
        else
        {
            trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
            Shield -= damage;
            if (trueDamage == 0)
            {
                DamageBlocked.Invoke();
            }
            else
            {
                Wounded.Invoke();
            }
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
        damage -= (int)(BaseDamageReduction * damage);
        if (damage <= 0) { return; }
        int trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
        Shield -= damage;
        if (trueDamage == 0)
        {
            DamageBlocked.Invoke();
        }
        else
        {
            Wounded.Invoke();
        }
        Health -= trueDamage;
        Damaged.Invoke();
        if (Health <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(int heal)
    {
        if (heal < 0) heal = 0;
        Health += heal;
    }

    public void AddShield(int shield)
    {
        Shield += shield;
    }
    public void ResetShield()
    {
        Shield = 0;
    }
    public void ResetEnergy()
    {
        Energy = maxBaseEnergy;
    }
    public virtual void ResetHP()
    {
        hp = maxHP;
    }
    public void GainEnergy(int energy)
    {
        Energy += energy;
    }

    public virtual void Die()
    {

    }

    public void ResetDeckPiles()
    {
        List<Card> auxList = new List<Card>();
        foreach (Card c in playedCards)
        {
            auxList.Add(c);
        }
        foreach (Card c in hand)
        {
            auxList.Add(c);
        }
        foreach (Card c in auxList)
        {
            DiscardCard(c);
        }
        List<Card> auxExaust = exausted.ToList();
        foreach (Card c in auxExaust)
        {
            DiscardCard(c);
            RevertExaust(c);
        }
        decks[0].ShuffleDeck();
    }
}
