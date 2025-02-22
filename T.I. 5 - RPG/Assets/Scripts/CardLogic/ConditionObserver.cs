using System.Collections.Generic;
using UnityEngine;

public class ConditionObserver : MonoBehaviour
{
    public static ConditionObserver observer;
    bool observing;
    void Awake()
    {
        /*if (observer == null)
        {
            observer = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }*/
        observer = this;
    }
    public static Combat currentCombat;
    public void StartObservation()
    {
        observing = true;
        currentCombat.creature1.PlayedCard.AddListener(CreaturePlayedCard);
        currentCombat.creature2.PlayedCard.AddListener(CreaturePlayedCard);
    }
    public void StopObservation() {
        observing = false;
        currentCombat.creature1.PlayedCard.RemoveListener(CreaturePlayedCard);
        currentCombat.creature2.PlayedCard.RemoveListener(CreaturePlayedCard);
    }
    public List<Condition> ObservedConditions = new List<Condition>(), CardRelatadConditions = new List<Condition>();
    public void AddCondition(Condition condition)
    {
        if (ObservedConditions.Contains(condition))
        {
            return;
        }
        ObservedConditions.Add(condition);
        switch (condition.type)
        {
            case Condition.ConditionType.CardRelatedCondition:
                CardRelatadConditions.Add(condition);
                break;
        }
        if (!observing && ObservedConditions.Count > 0) StartObservation();
    }
    public void RemoveCondition(Condition condition)
    {
        ObservedConditions.Remove(condition);
        switch (condition.type)
        {
            case Condition.ConditionType.CardRelatedCondition:
                CardRelatadConditions.Remove(condition);
                break;
        }
        if (observing && ObservedConditions.Count == 0) StopObservation();
    }

    public void CreaturePlayedCard(Card card)
    {
        foreach (Condition c in CardRelatadConditions)
        {
            c.CheckCondition(card);
        }
    }
}
