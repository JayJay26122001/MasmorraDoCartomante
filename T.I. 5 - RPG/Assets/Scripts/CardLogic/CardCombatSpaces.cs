using UnityEngine;

public class CardCombatSpaces : MonoBehaviour
{
    public Creature owner;
    public Transform buyingPileSpace, discardPileSpace, playerHandSpace, playedCardSpace;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(buyingPileSpace.position, 1);
        Gizmos.DrawSphere(discardPileSpace.position, 1);
        Gizmos.DrawSphere(playerHandSpace.position, 1);
        Gizmos.DrawSphere(playedCardSpace.position, 1);
    }
}
