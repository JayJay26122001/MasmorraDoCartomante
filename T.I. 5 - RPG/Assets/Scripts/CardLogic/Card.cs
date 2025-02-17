using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Card", menuName = "Scriptable Objects/CardLogic/Card")]
public class Card : ScriptableObject
{
    int cost;
    public UnityEvent CardEffect = new UnityEvent();
}
