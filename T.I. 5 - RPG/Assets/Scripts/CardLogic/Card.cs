using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Card", menuName = "CardLogic/Card")]
public class Card : ScriptableObject
{
    public Deck deck;
    public int cost;
    public enum CardType { Attack, Defense, Mind }
    public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }
    public CardType Type;
    public CardRarity Rarity;
    public string Description;
    public UnityEvent CardEffect = new UnityEvent();
}
