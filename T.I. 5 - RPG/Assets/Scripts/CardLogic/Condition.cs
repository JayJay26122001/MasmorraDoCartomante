using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class Condition
{
    //public enum condition { None, EnemyPlayedAttackFirst, EnemyPlayedDefenseFirst, EnemyPlayedMindFirst }
    protected bool ConditionActive = false;
    public enum ConditionState { Unsolved, Achieved, Failled }
    //public enum ConditionType { CardRelatedCondition }
    //public ConditionType type { get; private set; }
    //condition cond;
    public ConditionState ConditionStatus { get; private set; }
    //protected Card card;
    [System.NonSerialized] public Effect effect;
    public bool Repeatable = false; // if true the condition will repeat the efect every time it is acomplished

    //INICIALIZAÇÃO
    /*public Condition(condition condition, Card card)
    {
        cond = condition;
        this.card = card;
        SetConditionType();
    }
    void SetConditionType()
    {
        switch (cond)
        {
            case condition.EnemyPlayedAttackFirst | condition.EnemyPlayedDefenseFirst | condition.EnemyPlayedMindFirst:
                type = ConditionType.CardRelatedCondition;
                break;
        }
    }*/

    //OPÇÕES DE CONDIÇÃO

    /*public void ConfirmCondition() //confirma o sucesso da condição
    {
        ConditionStatus = ConditionState.Achieved;
        //TerminateCondition();
        //card.CheckConditions();
    }
    public void NeglectCondition() //confirma a falha da condição
    {
        ConditionStatus = ConditionState.Failled;
        //TerminateCondition();
        //card.CheckConditions();
    }*/
    /*public void TerminateCondition() //remove a condição do observer
    {
        ConditionObserver.observer.RemoveCondition(this);
    }*/
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
        //ConditionObserver.observer.AddCondition(this);
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
        effect.CheckConditions();
        DeactivateCondition();
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
    //CHECAGEM DA CONDIÇÃO
    /*public void CheckCondition(Card c)
    {
        switch (cond)
        {
            case condition.EnemyPlayedAttackFirst:
                PlayedCardTypeFirst(c, Card.CardType.Attack, card.deck.Owner.enemy);
                break;

            case condition.EnemyPlayedDefenseFirst:
                PlayedCardTypeFirst(c, Card.CardType.Defense, card.deck.Owner.enemy);
                break;

            case condition.EnemyPlayedMindFirst:
                PlayedCardTypeFirst(c, Card.CardType.Mind, card.deck.Owner.enemy);
                break;
        }
    }
    void PlayedCardTypeFirst(Card c, Card.CardType t, Creature cr)
    {
        if (c.deck.Owner == cr)
        {
            if (c.Type == t)
            {
                ConfirmCondition();
            }
            else
            {
                NeglectCondition();
            }
        }
    }*/
}
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
    [SerializeField] TurnPhase.PhaseTime PhaseTime;
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
