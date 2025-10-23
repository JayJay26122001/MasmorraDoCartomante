
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Unity.Mathematics;
[Serializable]
public abstract class Effect
{
    [System.NonSerialized] public Card card;
    [System.NonSerialized] public bool EffectAcomplished = false, effectStarted = false;
    [SerializeField] public bool DiscardIfAcomplished = false;
    [SerializeReference] public List<Condition> Conditions = new List<Condition>();
    [SerializeReference] public List<ConfirmationCondition> ConfirmationConditions = new List<ConfirmationCondition>();
    [System.NonSerialized] public UnityEvent EffectStart = new UnityEvent(), EffectEnd = new UnityEvent();
    public enum EffectState { Unsolved, InProgress, Acomplished, Failled }
    [NonSerialized] public EffectState state = EffectState.Unsolved;
    protected bool Repeatable = false;

    public void SetCard(Card c)
    {
        card = c;

        // Push card into all modular variables
        foreach (var field in GetType().GetFields(
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance))
        {
            if (typeof(ModularVar).IsAssignableFrom(field.FieldType))
            {
                var var = field.GetValue(this) as ModularVar;
                var?.SetCard(c);
            }
        }

        // Also assign to Conditions
        foreach (var cond in Conditions)
            cond.SetCard(c);
        foreach (var cond in ConfirmationConditions)
            cond.SetCard(c);

        /*if (this is IProlongedEffect e)
        {
            e.EffectApplied.AddListener(() => card.cardDisplay.SetActivatedEffectVFX(true));
            EffectEnd.AddListener(() => card.cardDisplay.SetActivatedEffectVFX(false));
        }
        else
        {
            EffectStart.AddListener(card.cardDisplay.PlayActivatedEffectOnce);
        }*/
    }

    public void InitiateEffect()
    {
        state = EffectState.Unsolved;
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
        state = EffectState.Unsolved;
        EffectAcomplished = false;
        effectStarted = false;
        foreach (Condition c in Conditions)
        {
            c.ResetCondition();
        }
    }
    public virtual void EffectEnded()
    {
        if (Repeatable)
        {
            EffectEnd.Invoke();
            return;
        }
        EffectAcomplished = true;
        EffectEnd.Invoke();
        if (!DiscardIfAcomplished)
        {
            foreach (Effect e in card.Effects)
            {
                if (!e.EffectAcomplished)
                {
                    return;
                }
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
                state = EffectState.Failled;
                EffectEnded();
                return;
            }
            else if (c.ConditionStatus == Condition.ConditionState.Unsolved/* && !(c is IConfirmationCondition)*/)
            {
                return;
            }
        }
        //state = EffectState.InProgress;
        ActionController.instance.AddToQueueBeforeAdvance(new ApplyEffectAction(this));
    }
    public void ApplyIfNoCondition()
    {
        if (Conditions.Count <= 0 && !effectStarted && !EffectAcomplished)
        {
            //state = EffectState.InProgress;
            ActionController.instance.AddToQueueBeforeAdvance(new ApplyEffectAction(this));
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
    public bool MultipliedByBaseDamage = true;
    public ModularFloat DamageMultiplier;
    [SerializeField] public bool IgnoreDefense;
    enum Target { Opponent, User }
    [SerializeField] Target target;
    public override void Apply()
    {
        base.Apply();
        DamageAction action = null;
        switch (target)
        {
            case Target.Opponent:
                //card.deck.Owner.enemy.TakeDamage(GetDamage(), IgnoreDefense);
                action = new DamageAction(card.deck.Owner.enemy, this);
                break;
            case Target.User:
                //card.deck.Owner.TakeDamage(GetDamage(), IgnoreDefense);
                action = new DamageAction(card.deck.Owner, this);
                break;
        }
        action.AnimEnded.AddListener(EffectEnded);
        //ActionController.instance.AddToQueue(action);
        action.StartAction();
        //EffectEnded();
    }
    public int GetDamage()
    {
        if (MultipliedByBaseDamage)
        {
            return (int)Math.Round(StatModifier.ApplyModfierList(card.deck.Owner.BaseDamage * DamageMultiplier.GetValue(), card.deck.Owner.DamageModifiers));
        }
        else
        {
            return (int)Math.Round(StatModifier.ApplyModfierList(DamageMultiplier.GetValue(), card.deck.Owner.DamageModifiers));
        }
    }
}

//[Serializable]public class GainDefense : GainShield{}
[Serializable]
public class GainShield : Effect
{
    enum Target { User, Opponent }
    public bool MultipliedByBaseShield = true;
    //[FormerlySerializedAs("DefenseMultiplier")]
    public ModularFloat ShieldMultiplier;
    [SerializeField] Target target;
    public override void Apply()
    {
        base.Apply();
        switch (target)
        {
            case Target.User:
                card.deck.Owner.AddShield(GetShield());
                break;
            case Target.Opponent:
                card.deck.Owner.enemy.AddShield(GetShield());
                break;
        }

        EffectEnded();
    }
    public int GetShield()
    {
        if (MultipliedByBaseShield)
        {
            return (int)Math.Round(StatModifier.ApplyModfierList(card.deck.Owner.BaseShieldGain * ShieldMultiplier.GetValue(), card.deck.Owner.ShieldModifiers));
        }
        else
        {
            return (int)Math.Round(StatModifier.ApplyModfierList(ShieldMultiplier.GetValue(), card.deck.Owner.ShieldModifiers));
        }

    }
}
[Serializable]
public class BuyCards : Effect
{
    public ModularInt BuyCardNumber;
    public override void Apply()
    {
        base.Apply();
        card.deck.Owner.BuyCards(BuyCardNumber.GetValue());
        EffectEnded();
        //Combat.WaitForTurn(0, GameplayManager.currentCombat.GetTurnPhase(card.deck.Owner, Combat.TurnPhaseTypes.Reaction), TurnPhase.PhaseTime.End, EffectEnded);
    }
}
public class GainEnergy : Effect
{
    public ModularInt Amount;
    enum Target { User, Opponent }
    [SerializeField] Target target;
    //enum GainTime { WhenPlayed, NextTurn };
    //[SerializeField] GainTime time;
    public override void Apply()
    {
        base.Apply();
        switch (target)
        {
            case Target.User:
                card.deck.Owner.GainEnergy(Amount.GetValue());
                break;
            case Target.Opponent:
                card.deck.Owner.enemy.GainEnergy(Amount.GetValue());
                break;
        }
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
    public ModularInt AmountHealled;
    enum Target { User, Opponent }
    [SerializeField] Target target;
    public override void Apply()
    {
        base.Apply();
        switch (target)
        {
            case Target.Opponent:
                card.deck.Owner.enemy.Heal(AmountHealled.GetValue());
                break;
            case Target.User:
                card.deck.Owner.Heal(AmountHealled.GetValue());
                break;
        }
        EffectEnded();
    }
}
[Serializable]
public class BuffStat : Effect, IProlongedEffect
{
    enum BuffableStats { Attack, ShieldGain, DamageTaken }
    [SerializeField] Target BuffTarget;
    [SerializeField] BuffableStats StatToBuff;
    public UnityEvent EffectApplied { get; set; }
    public StatModifier Modifier = new StatModifier();
    public BuffStat()
    {
        EffectApplied = new UnityEvent();
    }

    [Header("Duration")]
    [SerializeField] Target TurnOwner;
    enum Target { User, Opponent }
    public int TurnsFromNow;
    public Combat.TurnPhaseTypes TurnPhaseToStop;
    [SerializeField] TurnPhase.PhaseTime StopAtPhase;
    Creature owner, buffTar;
    List<StatModifier> StatBuffMod;
    TurnPhase phase;
    UnityAction Action;
    public override void Apply()
    {
        base.Apply();
        switch (TurnOwner)
        {
            case Target.Opponent:
                owner = card.deck.Owner.enemy;
                break;
            case Target.User:
                owner = card.deck.Owner;
                break;
        }
        switch (BuffTarget)
        {
            case Target.Opponent:
                buffTar = card.deck.Owner.enemy;
                break;
            case Target.User:
                buffTar = card.deck.Owner;
                break;
        }
        phase = GameplayManager.currentCombat.GetTurnPhase(owner, TurnPhaseToStop);
        if (StatBuffMod == null)
        {
            StatBuffMod = GetStatReference();
        }
        StatBuffMod.Add(Modifier);
        Action = Combat.WaitForTurn(TurnsFromNow, phase, StopAtPhase, EffectEnded);
        CardUIController.AttCardDescription(buffTar);
        GameplayManager.instance.UpdateStatsUI(buffTar);
        EffectApplied.Invoke();
    }
    public override void EffectEnded()
    {
        Combat.CancelWait(phase, StopAtPhase, Action);
        StatBuffMod?.Remove(Modifier);
        StatBuffMod = null;
        if (effectStarted)
        {
            CardUIController.AttCardDescription(buffTar);
        }

        base.EffectEnded();
    }
    private ref List<StatModifier> GetStatReference()
    {
        switch (StatToBuff)
        {
            case BuffableStats.Attack:
                return ref buffTar.DamageModifiers;
            case BuffableStats.ShieldGain:
                return ref buffTar.ShieldModifiers;
            case BuffableStats.DamageTaken:
                return ref buffTar.DamageReductionModifiers;
            default:
                throw new System.Exception("Unsupported stat type.");
        }
    }
}
public class ShuffleDeck : Effect //bota as cartas descartadas na pilha de compra e embaralha
{
    enum Target { User, Opponent }
    [SerializeField] Target target;
    public override void Apply()
    {
        base.Apply();
        switch (target)
        {
            case Target.Opponent:
                card.deck.Owner.enemy.decks[0].ShuffleDeck();
                break;
            case Target.User:
                card.deck.Owner.decks[0].ShuffleDeck();
                break;
        }
        EffectEnded();
    }
}
public class DiscardCard : Effect
{
    [SerializeField] ECardVar Cards = new ECardVar();
    public override void Apply()
    {
        base.Apply();
        foreach (Card c in Cards.GetCardsWithStats(card))
        {
            c.deck.Owner.DiscardCard(c);
        }
        EffectEnded();
    }
}
public class DestroyCard : Effect
{
    [SerializeField] ECardVar Cards = new ECardVar();
    public override void Apply()
    {
        base.Apply();
        foreach (Card c in Cards.GetCardsWithStats(card))
        {
            c.deck.Owner.ExaustCard(c);
        }
        EffectEnded();
    }
}
public class GainCoins : Effect
{
    enum Target { User, Opponent }
    enum Operation { Gain, StealFromOpponent }
    [SerializeField] Target target;
    [SerializeField] Operation Origin;
    [SerializeField] ModularInt amount;
    public override void Apply()
    {
        base.Apply();
        Creature t = null;
        switch (target)
        {
            case Target.User:
                t = card.deck.Owner;
                break;
            case Target.Opponent:
                t = card.deck.Owner.enemy;
                break;
        }
        switch (Origin)
        {
            case Operation.Gain:
                t.Money += amount.GetValue();
                break;
            case Operation.StealFromOpponent:
                int a = amount.GetValue();
                a = Math.Clamp(a, 0, t.enemy.Money);
                t.enemy.Money -= a;
                t.Money += a;
                break;
        }
        EffectEnded();
    }
}
public class CreateCard : Effect
{
    enum Target { User, Opponent }
    public enum Pile { Hand, BuyingPile, DiscardPile }
    [SerializeField] Target target;
    [SerializeField] Pile AddToPile;
    public Card CardPrefab;
    public ModularInt AmountOfInstances;
    public bool SetCostToZero = false;
    public override void Apply()
    {
        base.Apply();
        Creature t = null;
        switch (target)
        {
            case Target.User:
                t = card.deck.Owner;
                break;
            case Target.Opponent:
                t = card.deck.Owner.enemy;
                break;
        }
        //t.hand.Add(CardUIController.instance.InstantiateCard(CardPrefab).cardData);
        int aux = AmountOfInstances.GetValue();
        for (int i = 0; i < aux; i++)
        {
            Card inst = t.decks[0].AddTemporaryCard(CardPrefab, AddToPile);
            if (SetCostToZero)
            {
                inst.cost = 0;
            }
        }

        //CardUIController.OrganizeHandCards(t);
        EffectEnded();
    }
}

public class SetCardBoolProperty : Effect
{
    public ECardVar AffectedCards;
    public enum BooleanProperties { Limited, Instantaneous }
    public BooleanProperties SelectedBoolean;
    public bool booleanValue;
    public override void Apply()
    {
        base.Apply();
        List<Card> cards = AffectedCards.GetCardsWithStats(card);
        foreach (Card c in cards)
        {
            switch (SelectedBoolean)
            {
                case BooleanProperties.Limited:
                    c.limited = booleanValue;
                    break;
                case BooleanProperties.Instantaneous:
                    c.instantaneous = booleanValue;
                    break;
            }
            c.cardDisplay.UpdateCard();
        }
        EffectEnded();
    }
}
public class SetCardIntProperty : Effect
{
    public ECardVar AffectedCards;
    public enum IntProperties { Cost }
    public IntProperties SelectedInt;
    public ModularInt intValue;
    public override void Apply()
    {
        base.Apply();
        List<Card> cards = AffectedCards.GetCardsWithStats(card);
        foreach (Card c in cards)
        {
            switch (SelectedInt)
            {
                case IntProperties.Cost:
                    c.cost = intValue.GetValue();
                    break;
            }
            c.cardDisplay.UpdateCardCost();
        }
        EffectEnded();
    }
}
public class ResetProperties : Effect
{
    [System.Flags]
    public enum Vars
    {
        Limited = 1 << 0,
        Instantaneous = 1 << 1,
        Cost = 1 << 2,
    }
    public ECardVar AffectedCards;
    public Vars SelectedVar;
    public override void Apply()
    {
        base.Apply();
        List<Card> cards = AffectedCards.GetCardsWithStats(card);
        foreach (Card c in cards)
        {
            if ((SelectedVar & Vars.Limited) != 0)
            {
                c.RevertLimited();
            }
            if ((SelectedVar & Vars.Instantaneous) != 0)
            {
                c.RevertInstantaneous();
            }
            if ((SelectedVar & Vars.Cost) != 0)
            {
                c.RevertCost();
            }
        }

    }
}
public class SkipTurn : Effect
{
    enum Target { Opponent, User }
    public ModularInt AmountOfTurns;
    [SerializeField] Target target;
    public override void Apply()
    {
        base.Apply();
        switch (target)
        {
            case Target.Opponent:
                card.deck.Owner.enemy.SetToSkipTurn(AmountOfTurns.GetValue());
                break;
            case Target.User:
                card.deck.Owner.SetToSkipTurn(AmountOfTurns.GetValue());
                break;
        }
        EffectEnded();
    }
}
public class SkipBuyCard : Effect
{
    enum Target { Opponent, User }
    public ModularInt AmountOfTurns;
    [SerializeField] Target target;
    public override void Apply()
    {
        base.Apply();
        switch (target)
        {
            case Target.Opponent:
                card.deck.Owner.enemy.SetToSkipBuyCard(AmountOfTurns.GetValue());
                GameplayManager.instance.BlockDrawnVFX(card.deck.Owner.enemy);
                break;
            case Target.User:
                card.deck.Owner.SetToSkipBuyCard(AmountOfTurns.GetValue());
                GameplayManager.instance.BlockDrawnVFX(card.deck.Owner);
                break;
        }
        EffectEnded();
    }
}
public class DuplicateCard : Effect
{
    enum Target { User, Opponent }
    public enum Pile { Hand, BuyingPile, DiscardPile }
    [SerializeField] Target target;
    [SerializeField] Pile AddToPile;
    public CardVar CardsToCopy;
    public ModularInt AmountOfInstances;
    public bool SetCostToZero = false, SetInstantaneous = false, SetLimited = false;
    public override void Apply()
    {
        base.Apply();
        Creature t = null;
        switch (target)
        {
            case Target.User:
                t = card.deck.Owner;
                break;
            case Target.Opponent:
                t = card.deck.Owner.enemy;
                break;
        }
        //t.hand.Add(CardUIController.instance.InstantiateCard(CardPrefab).cardData);
        int aux = AmountOfInstances.GetValue();
        List<Card> cardsToinstance = CardsToCopy.GetCardsWithStats(card.deck.Owner);
        for (int i = 0; i < aux; i++)
        {
            foreach (Card c in cardsToinstance)
            {
                Card inst = t.decks[0].AddTemporaryCard(c, (CreateCard.Pile)AddToPile);
                if (SetCostToZero)
                {
                    inst.cost = 0;
                    inst.cardDisplay.UpdateCardCost();
                }
                if (SetInstantaneous)
                {
                    inst.instantaneous = true;
                }
                if (SetLimited)
                {
                    inst.limited = true;
                }
            }

        }

        //CardUIController.OrganizeHandCards(t);
        EffectEnded();
    }
}
