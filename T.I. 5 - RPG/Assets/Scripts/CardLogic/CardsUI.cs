using UnityEngine;

[CreateAssetMenu(fileName = "CardsUI", menuName = "CardLogic/CardsUI")]
public class CardsUI : ScriptableObject
{
    [Header("Sprite da Raridade da Carta")]
    public Sprite[] cardRarity = new Sprite[5];
    [Header("Sprite da Raridade da Carta")]
    public Sprite[] cardRarityBackground = new Sprite[5];
    [Header("Sprite do Tipo da Carta")]
    public Sprite[] cardType = new Sprite[3];
    [Header("Sprite do Tipo da Carta Especial de Diamante")]
    public Sprite[] cardDiamondType = new Sprite[3];
}
