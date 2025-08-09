using System;
using UnityEngine;

[Serializable]
public abstract class ConfirmationCondition
{
    protected Effect effect;
    public virtual void InitiateCondition()
    {

    }
}
[Serializable]
public class HasShield : ConfirmationCondition
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
}