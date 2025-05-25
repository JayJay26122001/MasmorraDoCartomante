
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
[Serializable]
public abstract class Effect
{
    [System.NonSerialized] public Card card;
    [System.NonSerialized] public bool EffectAcomplished = false, effectStarted = false;
    [SerializeReference] public List<Condition> Conditions;
    protected bool Repeatable = false;

    public void InitiateEffect()
    {
        foreach (Condition c in Conditions)
        {
            c.ActivateCondition();
            if (c.Repeatable)
            {
                Repeatable = true;
            }
        }
    }
    public virtual void Apply()
    {
        effectStarted = true;
    }
    public void resetEffect()
    {
        EffectAcomplished = false;
        effectStarted = false;
        foreach (Condition c in Conditions)
        {
            c.ResetCondition();
        }
    }
    public void EffectEnded()
    {
        if (Repeatable) return;
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
    public void CheckConditions() // Checa se as condições para este efeito foram resolvidas e aplica efeito se sim
    {
        if (EffectAcomplished) return;
        foreach (Condition c in Conditions)
        {
            if (c.ConditionStatus == Condition.ConditionState.Failled)
            {
                EffectEnded();
                return;
            }
            else if (c.ConditionStatus == Condition.ConditionState.Unsolved)
            {
                return;
            }
        }
        /*if (hidden)
        {
            hidden = false;
            CardUIController.OrganizeHandCards(deck.Owner);
        }
        CardHadEffect();*/
        Apply();
    }
    public void ApplyIfNoCondition()
    {
        if (Conditions.Count <= 0 && !effectStarted && !EffectAcomplished)
        {
            Apply();
        }
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
                card.deck.Owner.enemy.TakeDamage(GetDamage(), IgnoreDefense);
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
    enum GainTime { WhenPlayed, NextTurn };
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
public class DiscardThisCard : Effect
{
    public override void Apply()
    {
        base.Apply();
        card.deck.Owner.DiscardCard(card);
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
    [SerializeField]Target TurnOwner;
    enum Target { User, Oponent }
    public int TurnsFromNow;
    public Combat.TurnPhaseTypes TurnPhaseToStop;
    [SerializeField]TurnPhase.PhaseTime StopAtPhase;
    Creature owner;
    public override void Apply()
    {
        base.Apply();
        switch (TurnOwner)
        {
            case Target.Oponent:
                owner = card.deck.Owner.enemy;
                break;
            case Target.User:
                owner = card.deck.Owner;
                break;
        }
        TurnPhase phase = GameplayManager.currentCombat.GetTurnPhase(owner, TurnPhaseToStop);
        GetStatReference() *= MultiplicativeAmount;
        Combat.WaitForTurn(TurnsFromNow, phase, StopAtPhase, () => GetStatReference() /= MultiplicativeAmount);
        Combat.WaitForTurn(TurnsFromNow, phase, StopAtPhase, EffectEnded);
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