using UnityEngine;

public class Enemy : Creature
{
    public override void TurnAction()
    {
        BuyCards(1);
        PlayCard(hand[0]);
        GameplayManager.currentCombat.AdvanceCombat();
    }
}
