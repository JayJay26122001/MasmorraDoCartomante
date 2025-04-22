

public class Enemy : Creature
{
    public override void CombatStartAction()
    {
        base.CombatStartAction();
        BuyCards(1);
    }
    public override void TurnAction()
    {
        if (hand[0].Type == Card.CardType.Mind)
        {
            hand[0].hidden = true;
        }
        PlayCard(hand[0]);
        BuyCards(1);
        GameplayManager.currentCombat.AdvanceCombat();
    }
}
