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
    public int Damage;
    public override void Apply()
    {
        card.deck.Owner.Enemy.TakeDamage(Damage);
    }
}
[Serializable]
public class GainDefense : Effect
{
    //public DamageEffect(Card c) : base(c){}
    public int Defense;
    public override void Apply()
    {
        card.deck.Owner.AddShield(Defense);
    }
}
