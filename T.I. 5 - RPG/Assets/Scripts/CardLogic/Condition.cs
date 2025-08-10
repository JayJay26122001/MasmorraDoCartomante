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
    public bool Repeatable = false, DiscardIfAcomplished = false; // if true the condition will repeat the efect every time it is acomplished
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
        if (DiscardIfAcomplished)
        {
            effect.card.deck.Owner.DiscardCard(effect.card);
        }
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
    enum Target { Oponent, User }
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
            case Target.Oponent:
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
    [SerializeField]Target TurnOwner;
    enum Target { User, Oponent }
    public int TurnsFromNow;
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
            case Target.Oponent:
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
            ListenerAction = Combat.RepeatOnTurn(TurnsFromNow, phase, PhaseTime, ConditionCheck);
        }
        else
        {
            ListenerAction = Combat.WaitForTurn(TurnsFromNow, phase, PhaseTime, ConditionCheck);
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
    enum Target { User, Oponent }

    UnityAction ConditionToSuceed = null, ConditionToFail = null;
    Creature owner;
    public override void InitiateCondition()
    {
        base.InitiateCondition();
        switch (BlockedBy)
        {
            case Target.Oponent:
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
        if (Repeatable) return;
        ConditionToFail = () =>
        {
            ConcludeCondition(false);
        };
        owner.Wounded.AddListener(ConditionToFail);

    }
    protected override void Unsubscribe()
    {
        owner.DamageBlocked.RemoveListener(ConditionToSuceed);
        if (Repeatable) return;
        owner.Wounded.RemoveListener(ConditionToFail);
    }
}
/*[Serializable]
public class HasShield : Condition, IConfirmationCondition
{
    [SerializeField] Target CreatureObserved;
    enum Target { User, Oponent }
    [SerializeField] bool Reverse;
    Creature c;
    public override void InitiateCondition()
    {
        base.InitiateCondition();
        switch (CreatureObserved)
        {
            case Target.Oponent:
                c = effect.card.deck.Owner.enemy;
                break;
            case Target.User:
                c = effect.card.deck.Owner;
                break;
        }
    }
    public bool Confirm()
    {
        if (Reverse)
        {
            return !(c.Shield > 0);
        }
        else
        {
            return c.Shield > 0;
        }
    }
    protected override void Unsubscribe()
    {

    }

}*/
