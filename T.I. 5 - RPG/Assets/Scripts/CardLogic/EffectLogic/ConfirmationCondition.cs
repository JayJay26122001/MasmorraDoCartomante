using System;
using System.Collections.Generic;
using UnityEngine;
using static CardVar;

[Serializable]
public abstract class ConfirmationCondition
{
    protected enum Target { User, Opponent }
    protected enum Comparative { Higher, Lower, Iqual, IqualOrHigher, IqualOrLower }
    [NonSerialized] public Effect effect;
    public abstract bool Confirm();
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
}
[Serializable]
public class HasShield : ConfirmationCondition
{
    [SerializeField] Target CreatureObserved;
    [SerializeField] bool Reverse;
    Creature c;
    public override bool Confirm()
    {
        switch (CreatureObserved)
        {
            case Target.Opponent:
                c = effect.card.deck.Owner.enemy;
                break;
            case Target.User:
                c = effect.card.deck.Owner;
                break;
        }
        if (Reverse)
        {
            return !(c.Shield > 0);
        }
        else
        {
            return c.Shield > 0;
        }
    }
}
[Serializable]
public class ComparativeConfirmation : ConfirmationCondition
{
    [SerializeField] ModularInt ObservedValue;
    [SerializeField] Comparative Equation;
    [SerializeField] ModularInt Amount;
    //[SerializeField] Target CreatureObserved;
    Creature c;
    public override bool Confirm()
    {
        /*switch (CreatureObserved)
        {
            case Target.Opponent:
                c = effect.card.deck.Owner.enemy;
                break;
            case Target.User:
                c = effect.card.deck.Owner;
                break;
        }*/
        switch (Equation)
        {
            case Comparative.Higher:
                return ObservedValue.GetValue() > Amount.GetValue();
            case Comparative.Lower:
                return ObservedValue.GetValue() < Amount.GetValue();
            case Comparative.Iqual:
                return ObservedValue.GetValue() == Amount.GetValue();
            case Comparative.IqualOrHigher:
                return ObservedValue.GetValue() >= Amount.GetValue();
            case Comparative.IqualOrLower:
                return ObservedValue.GetValue() <= Amount.GetValue();
            default: return false;
        }
    }
}
[Serializable]
public class NumberOfTriggeredEffects : ConfirmationCondition
{
    public enum EffectState 
    { 
        Unsolved = 1 << 0, 
        InProgress = 1 << 1, 
        Acomplished = 1 << 2, 
        Failled = 1 << 3
    }
    public bool CountThisEffect;
    [SerializeField] Comparative Equation;
    [SerializeField] public ModularInt Amount;
    [SerializeField] EffectState EffectStates;
    int CompleteEffects;


    public override bool Confirm()
    {
        CompleteEffects = GetNumberOfCompleteEffects();
        switch (Equation)
        {
            case Comparative.Higher:
                return CompleteEffects > Amount.GetValue();
            case Comparative.Lower:
                return CompleteEffects < Amount.GetValue();
            case Comparative.Iqual:
                return CompleteEffects == Amount.GetValue();
            case Comparative.IqualOrHigher:
                return CompleteEffects >= Amount.GetValue();
            case Comparative.IqualOrLower:
                return CompleteEffects <= Amount.GetValue();
            default: return false;
        }
    }
    public int GetNumberOfCompleteEffects()
    {
        int num = 0;
        foreach (Effect e in effect.card.Effects)
        {
            if (e != effect || CountThisEffect)
            {
                Debug.Log(e.state);
                if ((EffectStates & (EffectState)(1 << (int)e.state)) != 0)
                {

                    num++;
                }
            }
        }
        Debug.Log(num);
        return num;
    }
}
public class RandomChance : ConfirmationCondition
{
    [Range(1, 99)] public float percentageChance;
    public override bool Confirm()
    {
        return   UnityEngine.Random.Range(1, 101) <= percentageChance;
    }
}