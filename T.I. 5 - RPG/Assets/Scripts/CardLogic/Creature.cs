using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEngine.UI;

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
    [Range(0, 200)][SerializeField] float baseDamageTaken = 100;
    public List<StatModifier> DamageModifiers, ShieldModifiers, DamageReductionModifiers;
    //public float BaseDamageMultiplier = 1, BaseDefenseMultiplier = 1;
    public UnityEvent<DealDamage> Damaged = new UnityEvent<DealDamage>(), Wounded = new UnityEvent<DealDamage>(), DamageBlocked = new UnityEvent<DealDamage>();
    public UnityEvent ShieldBreak = new UnityEvent(), GainedShield = new UnityEvent();
    public UnityEvent<Card> PlayedCard = new UnityEvent<Card>();
    public bool canPlayCards;
    public CardCombatSpaces combatSpace;

    public TextMeshPro hpText, shieldText, energyText;  //Ui das criaturas na batalha
    public UnityEngine.UI.Image hpCircle;
    public List<Card> exausted = new List<Card>();

    public int Money
    {
        get { return money; }
        set
        {
            if (value < 0) value = 0;
            money = value;
        }
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
        get { return baseDamage; }
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
        get { return baseShieldGain; }
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
    public float BaseDamageTaken
    {
        get
        {
            float res = baseDamageTaken;
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
            hpText.text = $"{c.Health}";
        }
        if (shieldText != null)
        {
            shieldText.text = $"{c.Shield}";
        }
        if (energyText != null)
        {
            energyText.text = $"{c.Energy}";
        }
        if(hpCircle != null)
        {
            hpCircle.fillAmount = (float)hp / (float)maxHP;
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
        PlayedCard.Invoke(c);
        c.CardPlayed();
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
        card.deck.BuyingPile.Remove(card);
        foreach (Effect e in card.Effects)
        {
            if (/*e.effectStarted &&*/ !e.EffectAcomplished)
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
            deck.RemoveTemporaryCards();
            foreach(Card card in deck.cards)
            {
                card.RevertAllProperties();
            }
            hand.Clear();
            //playedCards.Clear();
            deck.ResetPiles();
        }
        ResetEnergy();
        ResetShield();
        //ResetHP();
        ResetDamageModifiers();
        ResetShieldModifiers();
        CardUIController.AttDeckCard(this);
    }


    //COMBAT METHODS
    public virtual void TakeDamage(DealDamage dmg)
    {
        int damage = dmg.GetDamage();
        bool IgnoreDefense = dmg.IgnoreDefense;
        //damage -= (int)(BaseDamageTaken/100 * damage);
        damage = (int)(damage* (BaseDamageTaken / 100));
        if (damage <= 0) { return; }
        int trueDamage;
        if (IgnoreDefense)
        {
            trueDamage = damage;
            Wounded.Invoke(dmg);
        }
        else
        {
            trueDamage = (int)Mathf.Clamp(damage - Shield, 0, Mathf.Infinity);
            int OGshield = Shield;
            Shield -= damage;
            if (OGshield > 0 && Shield == 0)
            {
                ShieldBreak.Invoke();
            }
            if (trueDamage == 0)
            {
                DamageBlocked.Invoke(dmg);
            }
            else
            {
                Wounded.Invoke(dmg);
            }
        }
        Health -= trueDamage;
        Damaged.Invoke(dmg);
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
        int OGshield = Shield;
        Shield += shield;
        if (OGshield > 0 && OGshield + shield <= 0)
        {
            ShieldBreak.Invoke();
        }
        if (shield > 0)
        {
            GainedShield.Invoke();
        }
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
            RevertExaust(c);
            DiscardCard(c);
        }
        decks[0].ShuffleDeck();
    }
}
