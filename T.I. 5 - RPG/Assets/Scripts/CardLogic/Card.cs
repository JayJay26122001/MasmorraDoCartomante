using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Card", menuName = "CardLogic/Card")]
public class Card : ScriptableObject
{
    public void Setup()
    {
        foreach (Condition.condition c in conditions)
        {
            conds.Add(new Condition(c, this));
        }
    }
    public Deck deck;
    public string Name;
    public bool hidden;
    public int cost;
    public enum CardType { Attack, Defense, Mind }
    public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }
    public CardType Type;
    public CardRarity Rarity;
    public string Description;
    public List<Condition.condition> conditions = new List<Condition.condition>();
    List<Condition> conds = new List<Condition>();
    public void CardPlayed() // carta foi jogada na mesa
    {

    }


    //CONDICIONAIS DA CARTA
    public void CheckConditions() // Checa se as condições para os efeitos da carta foram resolvidas
    {
        foreach (Condition c in conds)
        {
            if (c.ConditionStatus == Condition.ConditionState.Failled)
            {
                ConditionalCardFailled();
                return;
            }
            else if (c.ConditionStatus == Condition.ConditionState.Unsolved)
            {
                return;
            }
        }
        CardHadEffect();
    }
    public void ConditionalCardFailled()// caso as condições da carta não tenham sido cumpridas
    {
        foreach (Condition c in conds)
        {
            c.TerminateCondition();
            c.ResetCondition();
        }
        deck.Owner.DiscardCard(this);
    }


    //EFEITOS DA CARTA
    public UnityEvent CardEffect = new UnityEvent();

    public void IniciateCardEffect() //tenta triggar o efeito da carta se ela não tiver condições, se tiver inicializa as condições
    {
        if (conditions.Count == 0)
        {
            CardHadEffect();
        }
        else
        {
            foreach (Condition c in conds)
            {
                c.InitiateCondition();
            }
        }
    }
    void CardHadEffect() //carta tem efeito
    {
        CardEffect.Invoke();
        foreach (Condition c in conds)
        {
            c.ResetCondition();
        }
        deck.Owner.DiscardCard(this);
    }

    public void DamageEnemy(int damage)
    {
        deck.Owner.Enemy.TakeDamage(damage);
    }
    public void AddDefense(int def)
    {
        deck.Owner.AddShield(def);
    }
}
