using System;
using UnityEngine;
[Serializable]
public abstract class Effect
{
    [System.NonSerialized] public Card card;
    [System.NonSerialized]public bool EffectAcomplished = false, effectStarted = false;
    /*public Effect(Card c)
    {
        card = c;
    }*/
    public virtual void Apply()
    {
        effectStarted = true;
    }
    public void resetEffect()
    {
        EffectAcomplished = false;
        effectStarted = false;
    }
    public void EffectEnded()
    {
        EffectAcomplished = true;
        foreach (Effect e in card.Effects)
        {
            if (!e.EffectAcomplished)
            {
                return;
            }
        }
        card.deck.Owner.DiscardCard(card);
    }
}
[Serializable]
public class DealDamage : Effect
{
    //public DamageEffect(Card c) : base(c){}
    public float DamageMultiplier;
    [SerializeField] bool IgnoreDefense;
    enum Target { Oponent, User }
    [SerializeField]Target target;
    public override void Apply()
    {
        base.Apply();
        switch (target)
        {
            case Target.Oponent:
                card.deck.Owner.Enemy.TakeDamage(GetDamage(), IgnoreDefense);
                break;
            case Target.User:
                card.deck.Owner.TakeDamage(GetDamage(), IgnoreDefense);
                break;
        }
        EffectEnded();
    }
    public int GetDamage()
    {
        return (int)Math.Ceiling(card.deck.Owner.BaseDamage * DamageMultiplier);
    }
}
[Serializable]
public class GainDefense : Effect
{
    //public DamageEffect(Card c) : base(c){}
    public float DefenseMultiplier;
    public override void Apply()
    {
        base.Apply();
        card.deck.Owner.AddShield(GetDefense());
        EffectEnded();
    }
    public int GetDefense()
    {
        return (int)Math.Ceiling(card.deck.Owner.BaseDefense * DefenseMultiplier);
    }
}
[Serializable]
public class BuyCards : Effect
{
    //public DamageEffect(Card c) : base(c){}
    public int BuyCardNumber;
    public override void Apply()
    {
        base.Apply();
        card.deck.Owner.BuyCards(BuyCardNumber);
        Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Reaction), TurnPhase.PhaseTime.End, EffectEnded);
    }
}
public class GainEnergy : Effect
{
    public int Amount;
    enum GainTime {WhenPlayed, NextTurn};
    [SerializeField] GainTime time;
    public override void Apply()
    {
        base.Apply();
        switch (time)
        {
            case GainTime.WhenPlayed:
                card.deck.Owner.GainEnergy(Amount);
                Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Reaction), TurnPhase.PhaseTime.End, EffectEnded);
                break;
            case GainTime.NextTurn:
                Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Start), TurnPhase.PhaseTime.Start, card.deck.Owner.GainEnergy, Amount);
                Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Start), TurnPhase.PhaseTime.Start, EffectEnded);
                break;
        }
        
    }
}
[Serializable]
public class BuffStatMultiplier : Effect
{
    enum BuffableStats { Attack, Defense }
    [SerializeField] BuffableStats StatToBuff;
    //enum BuffableMethod { Multiply, Add }
    //[SerializeField] BuffableMethod BuffMethod;
    public float MultiplicativeAmount;

    [Header("Duration")]
    public int TurnsFromNow;
    public Combat.TurnPhaseTypes TurnPhaseToStop;
    [SerializeField]TurnPhase.PhaseTime StopAtPhase;
    public override void Apply()
    {
        base.Apply();
        GetStatReference() *= MultiplicativeAmount;
        Combat.WaitForTurn(TurnsFromNow, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner,TurnPhaseToStop), StopAtPhase, () => GetStatReference() /= MultiplicativeAmount);
        Combat.WaitForTurn(TurnsFromNow, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner,TurnPhaseToStop), StopAtPhase, EffectEnded);
        /*switch (BuffMethod)
        {
            case BuffableMethod.Multiply:
                GetStatReference() *= MultiplicativeAmount;
                Combat.WaitForTurn(TurnsFromNow, GameplayManager.currentCombat.GetTurnPhase(TurnPhaseToStop), StopAtPhase, () => GetStatReference() /= MultiplicativeAmount);
                break;
            case BuffableMethod.Add:
                GetStatReference() += MultiplicativeAmount;
                break;
        }*/
    }
    private ref float GetStatReference()
    {
        switch (StatToBuff)
        {
            case BuffableStats.Attack:
                return ref card.deck.Owner.BaseDamageMultiplier;
            case BuffableStats.Defense:
                return ref card.deck.Owner.BaseDefenseMultiplier;
            default:
                throw new System.Exception("Unsupported stat type.");
        }
    }
}