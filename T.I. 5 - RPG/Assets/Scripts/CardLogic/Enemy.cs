using UnityEngine;

public class Enemy : Creature
{
    public override void CombatStartAction()
    {
        base.CombatStartAction();
        BuyCards(1);
    }
    public override void TurnAction()
    {
        PlayCard(hand[0]);
        BuyCards(1);
        GameplayManager.currentCombat.AdvanceCombat();
    }
}
