
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
[Serializable]
public abstract class Effect
{
    [System.NonSerialized] public Card card;
    [System.NonSerialized] public bool EffectAcomplished = false, effectStarted = false;
    [SerializeReference] public List<Condition> Conditions = new List<Condition>();
    [System.NonSerialized] public UnityEvent EffectStart = new UnityEvent(), EffectEnd = new UnityEvent();
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
        EffectStart.Invoke();
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
    public virtual void EffectEnded()
    {
        if (Repeatable) return;
        EffectAcomplished = true;
        EffectEnd.Invoke();
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
            else if (c.ConditionStatus == Condition.ConditionState.Unsolved && !(c is IConfirmationCondition))
            {
                return;
            }
        }
        foreach (IConfirmationCondition c in Conditions)
        {
            if (!c.Confirm())
            {
                EffectEnded();
                return;
            }
        }
        /*if (hidden)
        {
            hidden = false;
            CardUIController.OrganizeHandCards(deck.Owner);
        }
        CardHadEffect();*/
        //Apply();
        ActionController.instance.AddToQueue(new ApplyEffectAction(this));
    }
    public void ApplyIfNoCondition()
    {
        if (Conditions.Count <= 0 && !effectStarted && !EffectAcomplished)
        {
            //Apply();
            ActionController.instance.AddToQueue(new ApplyEffectAction(this));
        }
    }
}
public interface IActionEffect //Efeito que implementa uma ação
{

}
public interface IProlongedEffect // Efeito que dura por vários turnos e que, portanto, não se deve esperar a finalização
{
    public UnityEvent EffectApplied { get; set; }
}
public interface IHiddenEffect //Efeito que quando ativo não solta vfx ou indicações visuais
{
    
}

[Serializable]
public class DealDamage : Effect, IActionEffect
{
    public float DamageMultiplier;
    [SerializeField] bool IgnoreDefense;
    enum Target { Oponent, User }
    [SerializeField]Target target;
    public override void Apply()
    {
        base.Apply();
        DamageAction action = null;
        switch (target)
        {
            case Target.Oponent:
                //card.deck.Owner.enemy.TakeDamage(GetDamage(), IgnoreDefense);
                action = new DamageAction(card.deck.Owner.enemy, GetDamage(), IgnoreDefense);
                break;
            case Target.User:
                //card.deck.Owner.TakeDamage(GetDamage(), IgnoreDefense);
                action = new DamageAction(card.deck.Owner, GetDamage(), IgnoreDefense);
                break;
        }
        action.AnimEnded.AddListener(EffectEnded);
        //ActionController.instance.AddToQueue(action);
        action.StartAction();
        //EffectEnded();
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
        base.Apply();
        card.deck.Owner.AddShield(GetDefense());
        EffectEnded();
    }
    public int GetDefense()
    {
        return (int)Math.Round(card.deck.Owner.BaseShieldGain * DefenseMultiplier);
    }
}
[Serializable]
public class BuyCards : Effect
{
    public int BuyCardNumber;
    public override void Apply()
    {
        base.Apply();
        card.deck.Owner.BuyCards(BuyCardNumber);
        EffectEnded();
        //Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Reaction), TurnPhase.PhaseTime.End, EffectEnded);
    }
}
public class GainEnergy : Effect
{
    public int Amount;
    //enum GainTime { WhenPlayed, NextTurn };
    //[SerializeField] GainTime time;
    public override void Apply()
    {
        base.Apply();
        card.deck.Owner.GainEnergy(Amount);
        EffectEnded();
        /*Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Reaction), TurnPhase.PhaseTime.End, EffectEnded);
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
        }*/

    }
}
[Serializable]
public class DiscardThisCard : Effect, IHiddenEffect
{
    public enum DiscardType { Maximum, Minimum }
    public DiscardType SetAsDiscardTime;
    public override void Apply()
    {
        base.Apply();
        switch (SetAsDiscardTime)
        {
            case DiscardType.Maximum:
                card.deck.Owner.DiscardCard(card);
                break;
            case DiscardType.Minimum:
                if (CheckCompletion()) card.deck.Owner.DiscardCard(card);
                else { EffectEnded(); }
                break;
        }

    }
    bool CheckCompletion()
    {
        foreach (Effect e in card.Effects)
        {
            if (!e.EffectAcomplished && e != this)
            {
                return false;
            }
        }
        return true;
    }
}
[Serializable]
public class Heal : Effect
{
    public int AmountHealled;
    enum Target { User, Oponent }
    [SerializeField]Target target;
    public override void Apply()
    {
        base.Apply();
        switch (target)
        {
            case Target.Oponent:
                card.deck.Owner.enemy.Heal(AmountHealled);
                break;
            case Target.User:
                card.deck.Owner.Heal(AmountHealled);
                break;
        }
        EffectEnded();
    }
}
[Serializable]
public class BuffStat : Effect, IProlongedEffect
{
    enum BuffableStats { Attack, ShieldGain, DamageReduction }
    [SerializeField] BuffableStats StatToBuff;
    public UnityEvent EffectApplied{ get; set; }
    public StatModifier Modifier = new StatModifier();
    public BuffStat()
    {
        EffectApplied = new UnityEvent();
    }

    [Header("Duration")]
    [SerializeField] Target TurnOwner;
    enum Target { User, Oponent }
    public int TurnsFromNow;
    public Combat.TurnPhaseTypes TurnPhaseToStop;
    [SerializeField] TurnPhase.PhaseTime StopAtPhase;
    Creature owner;
    List<StatModifier> StatBuffMod;
    TurnPhase phase;
    UnityAction Action;
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
        phase = GameplayManager.currentCombat.GetTurnPhase(owner, TurnPhaseToStop);
        if (StatBuffMod == null)
        {
            StatBuffMod = GetStatReference();
        }
        StatBuffMod.Add(Modifier);
        Action = Combat.WaitForTurn(TurnsFromNow, phase, StopAtPhase, EffectEnded);
        EffectApplied.Invoke();
    }
    public override void EffectEnded()
    {
        Combat.CancelWait(phase, StopAtPhase, Action);
        StatBuffMod?.Remove(Modifier);
        StatBuffMod = null;
        base.EffectEnded();
    }
    private ref List<StatModifier> GetStatReference()
    {
        switch (StatToBuff)
        {
            case BuffableStats.Attack:
                return ref owner.DamageModifiers;
            case BuffableStats.ShieldGain:
                return ref owner.ShieldModifiers;
            case BuffableStats.DamageReduction:
                return ref owner.DamageReductionModifiers;
            default:
                throw new System.Exception("Unsupported stat type.");
        }
    }
}
