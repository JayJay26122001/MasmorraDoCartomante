using System;
using UnityEngine;

[Serializable]
public abstract class ConfirmationCondition
{
    protected enum Target { User, Oponent }
    protected enum Comparative { Higher, Lower, Iqual, IqualOrHigher, IqualOrLower }
    [NonSerialized] public Effect effect;
    public abstract bool Confirm();
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
            case Target.Oponent:
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
public class HasHealthAmount : ConfirmationCondition
{
    [SerializeField] int HealthAmount;
    [SerializeField] Target CreatureObserved;
    [SerializeField] Comparative Equation;
    Creature c;
    public override bool Confirm()
    {
        switch (CreatureObserved)
        {
            case Target.Oponent:
                c = effect.card.deck.Owner.enemy;
                break;
            case Target.User:
                c = effect.card.deck.Owner;
                break;
        }
        switch (Equation)
        {
            case Comparative.Higher:
                return c.Health > HealthAmount;
            case Comparative.Lower:
                return c.Health < HealthAmount;
            case Comparative.Iqual:
                return c.Health == HealthAmount;
            case Comparative.IqualOrHigher:
                return c.Health >= HealthAmount;
            case Comparative.IqualOrLower:
                return c.Health <= HealthAmount;
            default: return false;
        }
    }
}
[Serializable]
public class NumberOfTriggeredEffects : ConfirmationCondition
{
    [SerializeField] Comparative Equation;
    [SerializeField]public int Amount;
    int CompleteEffects;

    public override bool Confirm()
    {
        CompleteEffects = GetNumberOfCompleteEffects();
        switch (Equation)
        {
            case Comparative.Higher:
                return Amount > CompleteEffects;
            case Comparative.Lower:
                return Amount < CompleteEffects;
            case Comparative.Iqual:
                return Amount == CompleteEffects;
            case Comparative.IqualOrHigher:
                return Amount >= CompleteEffects;
            case Comparative.IqualOrLower:
                return Amount <= CompleteEffects;
            default: return false;
        }
    }
    public int GetNumberOfCompleteEffects()
    {
        int num = 0;
        foreach (Effect e in effect.card.Effects)
        {
            if (e.EffectAcomplished)
            {
                num++;
            }
        }
        return num;
    }
}