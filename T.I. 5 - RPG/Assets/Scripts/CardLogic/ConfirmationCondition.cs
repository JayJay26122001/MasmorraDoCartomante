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
public class HasHealthAmount: ConfirmationCondition
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