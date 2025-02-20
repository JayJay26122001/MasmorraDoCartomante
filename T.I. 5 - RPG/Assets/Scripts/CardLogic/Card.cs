using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Card", menuName = "CardLogic/Card")]
public class Card : ScriptableObject
{
    public void Setup()
    {
        foreach (Condition.condition c in conditions) {
            new Condition(c, this);
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
    public void CardPlayed()
    {
        if (conditions.Count == 0)
        {
            CardEffect.Invoke();
        }
        else
        {
            foreach (Condition c in conds)
            {
                c.InitiateCondition();
            }
        }
    }
    public void CheckConditions()
    {
        foreach (Condition c in conds)
        {
            if (!c.ConditionAchieved)
            {
                return;
            }
        }
        CardEffect.Invoke();
    }
    public UnityEvent CardEffect = new UnityEvent();

    public void DamageEnemy(int damage)
    {
        deck.Owner.Enemy.TakeDamage(damage);
    }
    public void AddDefense(int def)
    {
        deck.Owner.AddShield(def);
    }
}
