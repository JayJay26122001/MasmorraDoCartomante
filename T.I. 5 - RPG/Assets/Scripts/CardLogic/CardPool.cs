using System.Collections.Generic;
using UnityEditor;
//using UnityEditor.ShaderGraph.Internal;
//using UnityEditor.UIElements;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "CardPool", menuName = "CardLogic/CardPool")]
public class CardPool : ScriptableObject
{
    public List<Card> pool = new List<Card>();
    public List<float> baseProbabilities = new List<float>();
    List<float> probabilities = new List<float>();

    [System.Flags]
    public enum CardType { Attack, Defense, Mind }
    [System.Flags]
    public enum CardPack { Normal, Zodiac, EnemyExclusive, MinorArcana, MajorArcana, SandsOfTime, PowerSurge }
    [System.Flags]
    public enum CardRarity { Common, Uncommon, Rare, Epic, Legendary }

    [DrawerAttribute]
    public CardType type;
    [DrawerAttribute]
    public CardRarity rarity;
    [DrawerAttribute]
    public CardPack packSet;

    List<int> rarityAux = new List<int>();

    [ContextMenu("Set Pool")]
    public void SetPool()
    {
        rarityAux.Clear();
        pool.Clear();
        //probabilities.Clear();
        List<int> selectedTypes = DrawerAttribute.GetSelectedIndexes(type);
        List<int> selectedRarities = DrawerAttribute.GetSelectedIndexes(rarity);
        List<int> selectedSets = DrawerAttribute.GetSelectedIndexes(packSet);
        Card[] aux = Resources.LoadAll<Card>("");
        for(int i = 0; i < aux.Length; i++)
        {
            if (selectedTypes.Contains((int)aux[i].Type) && selectedRarities.Contains((int)aux[i].Rarity) && selectedSets.Contains((int)aux[i].Pack))
            {
                pool.Add(aux[i]);
                //probabilities.Add(GetProbability(aux[i]));
            }
        }
        AdaptProbabilities();
    }

    /*int GetProbability(Card card)
    {
        int prob = 0;
        switch (card.Rarity)
        {
            case Card.CardRarity.Common:
                prob = 30;
                break;

            case Card.CardRarity.Uncommon:
                prob = 20;
                break;

            case Card.CardRarity.Rare:
                prob = 10;
                break;

            case Card.CardRarity.Epic:
                prob = 5;
                break;

            case Card.CardRarity.Legendary:
                prob = 1;
                break;
        }
        return prob;
    }*/

    public void AdaptProbabilities()
    {
        rarityAux.Clear();
        for (int i = 0; i < pool.Count; i++)
        {
            rarityAux.Add((int)pool[i].Rarity);
        }
        probabilities.Clear();
        for(int i = 0; i < baseProbabilities.Count; i++)
        {
            probabilities.Insert(i, baseProbabilities[i]);
        }
        //List<int> rarities = DrawerAttribute.GetSelectedIndexes(rarity);
        float aux;
        for (int i = 0; i < probabilities.Count; i++)
        {
            if (!rarityAux.Contains(i))
            {
                aux = 100 - probabilities[i];
                for (int j = 0; j < probabilities.Count; j++)
                {
                    if (j != i)
                    {
                        probabilities[j] = (probabilities[j] / aux) * 100;
                    }
                }
                probabilities[i] = 0;
            }
        }

        /*for(int i = 0; i < probabilities.Count; i++)
        {
            Debug.Log("Probabilidade de carta " + (Card.CardRarity)i + ": " +  probabilities[i]);
        }*/
    }

    public int SelectRarity()
    {
        float rand = UnityEngine.Random.Range(0, 100);
        float aux = 0;
        int result = 0;
        for (int i = 0; i < probabilities.Count; i++)
        {
            if (rarityAux.Contains(i))
            {
                aux += probabilities[i];
                if(rand < aux)
                {
                    result = i;
                    i = probabilities.Count;
                }
            }
        }
        return result;
    }

    public List<Card> SelectCards(int quantity)
    {
        AdaptProbabilities();
        List<Card> cards = new List<Card>();
        List<Card> auxList = new List<Card>();
        while(cards.Count < quantity)
        {
            bool success = false;
            while(!success)
            {
                auxList.Clear();
                int chosenRarity = SelectRarity();
                foreach (Card c in pool)
                {
                    if ((int)c.Rarity == chosenRarity)
                    {
                        auxList.Add(c);
                    }
                }
                int rand = UnityEngine.Random.Range(0, auxList.Count);
                if (!cards.Contains(auxList[rand]))
                {
                    cards.Add(auxList[rand]);
                    success = true;
                }
            }
        }
        return cards;
    }
    /*public List<Card> SelectCards(int quantity)
    {
        List<Card> auxList = new List<Card>();
        int sum = 0;
        foreach(int s in probabilities)
        {
            sum += s;
        }
        while(auxList.Count < quantity)
        {
            bool success = false;
            while(!success)
            {
                int rand = UnityEngine.Random.Range(0, sum);
                int aux = 0;
                for(int i = 0; i < probabilities.Count; i++)
                {
                    aux += probabilities[i];
                    if(rand < aux)
                    {
                        if(!auxList.Contains(pool[i]))
                        {
                            auxList.Add(pool[i]);
                            success = true;
                        }
                        i = probabilities.Count;
                    }
                }
            }
        }
        return auxList;
    }*/
}
public class DrawerAttribute: PropertyAttribute
{
    public DrawerAttribute()
    {

    }

    public static List<string> GetSelectedStrings<T>(T v)
    {
        List<string> selected = new List<string>();
        for(int i = 0; i < Enum.GetValues(typeof(T)).Length; i++)
        {
            int layer = 1 << i;
            if((Convert.ToInt32(v) & layer) != 0)
            {
                selected.Add(Enum.GetValues(typeof(T)).GetValue(i).ToString());
            }
        }
        return selected;
    }

    public static List<int> GetSelectedIndexes<T>(T v)
    {
        List<int> selected = new List<int>();
        for(int i = 0; i < Enum.GetValues(typeof(T)).Length; i++)
        {
            int layer = 1 << i;
            if((Convert.ToInt32(v) & layer) != 0)
            {
                selected.Add(i);
            }
        }
        return selected;
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DrawerAttribute))]
public class Drawer: PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        prop.intValue = EditorGUI.MaskField(pos, label, prop.intValue, prop.enumNames);
    }
}
#endif

