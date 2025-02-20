using UnityEngine;

public class Condition
{
    public enum condition { None, EnemyPlayedAttack, EnemyPlayedDefense, EnemyPlayedMind }
    condition cond;
    Card card;
    public bool ConditionAchieved { get; private set; }
    public Condition(condition condition, Card card)
    {
        cond = condition;
        this.card = card;
    }
    public void InitiateCondition()
    {
        ConditionAchieved = false;
        AddConditionToEvent();
    }
    void AddConditionToEvent()
    {
        switch (cond)
        {
            case condition.EnemyPlayedAttack:
                card.deck.Owner.Enemy.PlayedCard.AddListener(CardIsAttack);
                break;
            case condition.EnemyPlayedDefense:
                card.deck.Owner.Enemy.PlayedCard.AddListener(CardIsDefense);
                break;
            case condition.EnemyPlayedMind:
                card.deck.Owner.Enemy.PlayedCard.AddListener(CardIsMind);
                break;
        }
    }
    void CardIsAttack(Card card)
    {
        if (card.Type == Card.CardType.Attack) { AchieveCondition(); card.deck.Owner.Enemy.PlayedCard.RemoveListener(CardIsAttack); }
    }
    void CardIsDefense(Card card)
    {
        if (card.Type == Card.CardType.Defense) { AchieveCondition(); card.deck.Owner.Enemy.PlayedCard.RemoveListener(CardIsDefense);}
    }
    void CardIsMind(Card card)
    {
        if (card.Type == Card.CardType.Mind) { AchieveCondition(); card.deck.Owner.Enemy.PlayedCard.RemoveListener(CardIsMind); }
    }
    void AchieveCondition()
    {
        ConditionAchieved = true;
        card.CheckConditions();
    }
}
