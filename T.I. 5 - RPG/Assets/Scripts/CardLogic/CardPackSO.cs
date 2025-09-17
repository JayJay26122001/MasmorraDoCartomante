using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "CardPackSO", menuName = "CardLogic/CardPackSO")]
public class CardPackSO : ScriptableObject
{
    public CardPool possibleCards;
    public int price, cardQuantity, selectableCardsQuantity;
    [TextArea] public string packDescription;
}
