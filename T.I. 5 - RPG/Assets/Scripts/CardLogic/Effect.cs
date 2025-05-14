using System;
using UnityEngine;
[Serializable]
public abstract class Effect
{
    [System.NonSerialized]public Card card;
    /*public Effect(Card c)
    {
        card = c;
    }*/
    public abstract void Apply();
}
[Serializable]
public class DealDamage : Effect
{
    //public DamageEffect(Card c) : base(c){}
    public float DamageMultiplier;
    public override void Apply()
    {
        card.deck.Owner.Enemy.TakeDamage(GetDamage());
    }
    public int GetDamage()
    {
        return (int)Math.Round(card.deck.Owner.BaseDamage * DamageMultiplier);
    }
}
[Serializable]
public class GainDefense : Effect
{
    //public DamageEffect(Card c) : base(c){}
    public float DefenseMultiplier;
    public override void Apply()
    {
        card.deck.Owner.AddShield(GetDefense());
    }
    public int GetDefense()
    {
        return (int)Math.Round(card.deck.Owner.BaseDefense * DefenseMultiplier);
    }
}
