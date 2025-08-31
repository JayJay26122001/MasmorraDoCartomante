using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class Condition
{
    protected bool ConditionActive = false;
    public enum ConditionState { Unsolved, Achieved, Failled }
    public ConditionState ConditionStatus { get; private set; }
    [System.NonSerialized] public Effect effect;
    public bool Repeatable = false/*, DiscardIfAcomplished = false*/; // if true the condition will repeat the efect every time it is acomplished
    public virtual void SetCard(Card c)
    {
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
    }
    public void ActivateCondition() //começa a observar a condição
    {
        if (!ConditionActive)
        {
            InitiateCondition();
            ConditionActive = true;
        }

    }
    public virtual void InitiateCondition()
    {
        ResetCondition();
    }
    public void ConcludeCondition(bool Achieved)
    {
        if (Repeatable)
        {
            if (Achieved)
            {
                ConditionStatus = ConditionState.Achieved;
                effect.CheckConditions();
                ConditionStatus = ConditionState.Unsolved;
            }
            return;
        }
        if (Achieved)
        {
            ConditionStatus = ConditionState.Achieved;
        }
        else
        {
            ConditionStatus = ConditionState.Failled;
        }
        DeactivateCondition();
        effect.CheckConditions();
        /*if (DiscardIfAcomplished)
        {
            effect.card.deck.Owner.DiscardCard(effect.card);
        }*/
    }
    public void ResetCondition() //reseta o status da condição
    {
        ConditionStatus = ConditionState.Unsolved;
        DeactivateCondition();
    }
    public void DeactivateCondition()
    {
        if (ConditionActive)
        {
            Unsubscribe();
            ConditionActive = false;
        }
    }
    protected abstract void Unsubscribe();
}
/*public interface IConfirmationCondition // essas condições não impactam o tempo de ativação, serão apenas determinantes de se o efeito será ou não ativado
{
    public bool Confirm();
}*/
[Serializable]
public class CreaturePlayedCardType : Condition
{
    enum Target { Opponent, User }
    [SerializeField] Target target;
    Creature t;
    [SerializeField] private Card.CardType expectedType;
    [SerializeField] private bool mustBeFirst;
    UnityAction<Card> ConditionCheck = null;
    public override void InitiateCondition()
    {
        base.InitiateCondition();
        switch (target)
        {
            case Target.Opponent:
                t = effect.card.deck.Owner.enemy;
                break;
            case Target.User:
                t = effect.card.deck.Owner;
                break;
        }
        ConditionCheck = (Card c) =>
        {
            if (c.Type == expectedType)
            {
                ConcludeCondition(true);
            }
            else if (mustBeFirst)
            {
                ConcludeCondition(false);
            }
        };
        t.PlayedCard.AddListener(ConditionCheck);
    }
    protected override void Unsubscribe()
    {
        t.PlayedCard.RemoveListener(ConditionCheck);
    }
}

[Serializable]
public class WaitUntilTurn : Condition
{
    [SerializeField] Target TurnOwner;
    enum Target { User, Opponent }
    public ModularInt TurnsFromNow;
    public Combat.TurnPhaseTypes TurnPhase;
    public TurnPhase.PhaseTime PhaseTime;
    UnityAction ConditionCheck = null, ListenerAction;
    Creature owner;
    TurnPhase phase;
    public override void InitiateCondition()
    {
        base.InitiateCondition();
        switch (TurnOwner)
        {
            case Target.Opponent:
                owner = effect.card.deck.Owner.enemy;
                break;
            case Target.User:
                owner = effect.card.deck.Owner;
                break;
        }
        phase = GameplayManager.currentCombat.GetTurnPhase(owner, TurnPhase);

        ConditionCheck = () =>
        {
            ConcludeCondition(true);
        };
        if (Repeatable)
        {
            ListenerAction = Combat.RepeatOnTurn(TurnsFromNow.GetValue(), phase, PhaseTime, ConditionCheck);
        }
        else
        {
            ListenerAction = Combat.WaitForTurn(TurnsFromNow.GetValue(), phase, PhaseTime, ConditionCheck);
        }

    }
    protected override void Unsubscribe()
    {
        Combat.CancelWait(phase, PhaseTime, ListenerAction);
    }
}
[Serializable]
public class DamageBlocked : Condition
{
    [SerializeField] Target BlockedBy;
    [SerializeField] bool FailIfDamaged;
    enum Target { User, Opponent }

    UnityAction ConditionToSuceed = null, ConditionToFail = null;
    Creature owner;
    public override void InitiateCondition()
    {
        base.InitiateCondition();
        switch (BlockedBy)
        {
            case Target.Opponent:
                owner = effect.card.deck.Owner.enemy;
                break;
            case Target.User:
                owner = effect.card.deck.Owner;
                break;
        }

        ConditionToSuceed = () =>
        {
            ConcludeCondition(true);
        };
        owner.DamageBlocked.AddListener(ConditionToSuceed);
        if (FailIfDamaged)
        {
            ConditionToFail = () =>
            {
                ConcludeCondition(false);
            };
            owner.Wounded.AddListener(ConditionToFail);
        }
    }
    protected override void Unsubscribe()
    {
        owner.DamageBlocked.RemoveListener(ConditionToSuceed);
        if (FailIfDamaged)
        {
            owner.Wounded.RemoveListener(ConditionToFail);
        }
    }
}
[Serializable]
public class DamageTaken : Condition
{
    [SerializeField] Target DamagedTarget;
    [SerializeField] bool CountShieldedDamage;
    enum Target { User, Opponent }

    UnityAction ConditionToSuceed = null;
    UnityEvent chosenEvent = null;
    Creature owner;
    public override void InitiateCondition()
    {
        base.InitiateCondition();
        switch (DamagedTarget)
        {
            case Target.Opponent:
                owner = effect.card.deck.Owner.enemy;
                break;
            case Target.User:
                owner = effect.card.deck.Owner;
                break;
        }
        if (CountShieldedDamage)
        {
            chosenEvent = owner.Damaged;
        }
        else
        {
            chosenEvent = owner.Wounded;
        }
        ConditionToSuceed = () =>
        {
            ConcludeCondition(true);
        };
        chosenEvent.AddListener(ConditionToSuceed);
    }
    protected override void Unsubscribe()
    {
        chosenEvent.RemoveListener(ConditionToSuceed);
    }
}
public class ShieldBroke : Condition
{
    [SerializeField] Target target;
    enum Target { User, Opponent }

    UnityAction ConditionToSuceed = null;
    Creature owner;
    public override void InitiateCondition()
    {
        base.InitiateCondition();
        switch (target)
        {
            case Target.Opponent:
                owner = effect.card.deck.Owner.enemy;
                break;
            case Target.User:
                owner = effect.card.deck.Owner;
                break;
        }

        ConditionToSuceed = () =>
        {
            ConcludeCondition(true);
        };
        owner.ShieldBreak.AddListener(ConditionToSuceed);
    }
    protected override void Unsubscribe()
    {
        owner.ShieldBreak.RemoveListener(ConditionToSuceed);
    }
}
