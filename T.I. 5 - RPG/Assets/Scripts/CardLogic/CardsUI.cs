using UnityEngine;

[CreateAssetMenu(fileName = "CardsUI", menuName = "CardLogic/CardsUI")]
public class CardsUI : ScriptableObject
{
    [Header("Sprite da Raridade da Carta")]
    public Sprite[] cardRarity = new Sprite[5];
    [Header("Sprite do Tipo da Carta")]
    public Sprite[] cardType = new Sprite[3];
}
