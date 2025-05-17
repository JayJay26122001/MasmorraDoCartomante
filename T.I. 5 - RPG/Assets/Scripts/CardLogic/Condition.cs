using System;
using UnityEngine;

[Serializable]
public abstract class Condition
{
    public enum condition { None, EnemyPlayedAttackFirst, EnemyPlayedDefenseFirst, EnemyPlayedMindFirst }
    public enum ConditionState { Unsolved, Achieved, Failled }
    public enum ConditionType { CardRelatedCondition }
    public ConditionType type { get; private set; }
    condition cond;
    public ConditionState ConditionStatus { get; private set; }
    Card card;

    //INICIALIZAÇÃO
    /*public Condition(condition condition, Card card)
    {
        cond = condition;
        this.card = card;
        SetConditionType();
    }*/
    void SetConditionType()
    {
        switch (cond)
        {
            case condition.EnemyPlayedAttackFirst | condition.EnemyPlayedDefenseFirst | condition.EnemyPlayedMindFirst:
                type = ConditionType.CardRelatedCondition;
                break;
        }
    }

    //OPÇÕES DE CONDIÇÃO
    public void InitiateCondition() //começa a observar a condição
    {
        ResetCondition();
        ConditionObserver.observer.AddCondition(this);
    }
    public void ConfirmCondition() //confirma o sucesso da condição
    {
        ConditionStatus = ConditionState.Achieved;
        TerminateCondition();
        //card.CheckConditions();
    }
    public void NeglectCondition() //confirma a falha da condição
    {
        ConditionStatus = ConditionState.Failled;
        TerminateCondition();
        //card.CheckConditions();
    }
    public void TerminateCondition() //remove a condição do observer
    {
        ConditionObserver.observer.RemoveCondition(this);
    }
    public void ResetCondition() //reseta o status da condição
    {
        ConditionStatus = ConditionState.Unsolved;
    }


    //CHECAGEM DA CONDIÇÃO
    public void CheckCondition(Card c)
    {
        switch (cond)
        {
            case condition.EnemyPlayedAttackFirst:
                PlayedCardTypeFirst(c, Card.CardType.Attack, card.deck.Owner.Enemy);
                break;

            case condition.EnemyPlayedDefenseFirst:
                PlayedCardTypeFirst(c, Card.CardType.Defense, card.deck.Owner.Enemy);
                break;

            case condition.EnemyPlayedMindFirst:
                PlayedCardTypeFirst(c, Card.CardType.Mind, card.deck.Owner.Enemy);
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
    }
}
[Serializable]
public class CreaturePlayedCardType : Condition
{
    [SerializeField] private Card.CardType expectedType;
    [SerializeField] private bool mustBeFirst;
}
