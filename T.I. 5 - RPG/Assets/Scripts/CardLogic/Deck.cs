using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "Scriptable Objects/CardLogic/Deck")]
public class Deck : ScriptableObject
{
    public List<Card> cards = new List<Card>();
    public Stack<Card> BuyingPile, DiscardPile;
    public void ShuffleDeck()
    {
        BuyingPile.Clear();
        DiscardPile.Clear();
        BuyingPile = ToStack(Shuffle(cards));
    }
    public void ShufflePile(ref Stack<Card> pile){
        Shuffle(pile);
    }
    public static List<T> Shuffle<T>(List<T> list)
    {
        for (int n = list.Count; n > 1;)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
    public static Stack<T> Shuffle<T>(Stack<T> stack)
    {
        List<T> list = stack.ToList();
        stack.Clear();

        for (int n = stack.Count; n > 1;)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return ToStack(list);

    }
    public static Stack<T> ToStack<T>(List<T> list)
    {
        Stack<T> stack = new Stack<T>();
        foreach (T v in list)
        {
            stack.Push(v);
        }
        return stack;
    }
}
