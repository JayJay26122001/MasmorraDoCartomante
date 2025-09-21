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
    public List<int> probabilities = new List<int>();

    [DrawerAttribute]
    public Card.CardType type;
    [DrawerAttribute]
    public Card.CardRarity rarity;
    [DrawerAttribute]
    public Card.CardPack packSet;

    [ContextMenu("Set Pool")]
    public void SetPool()
    {
        pool.Clear();
        probabilities.Clear();
        List<int> selectedTypes = DrawerAttribute.GetSelectedIndexes(type);
        List<int> selectedRarities = DrawerAttribute.GetSelectedIndexes(rarity);
        List<int> selectedSets = DrawerAttribute.GetSelectedIndexes(packSet);
        Card[] aux = Resources.LoadAll<Card>("");
        //Debug.Log(aux.Length);
        for(int i = 0; i < aux.Length; i++)
        {
            if (selectedTypes.Contains((int)aux[i].Type) && selectedRarities.Contains((int)aux[i].Rarity) && selectedSets.Contains((int)aux[i].Pack))
            {
                pool.Add(aux[i]);
                probabilities.Add(GetProbability(aux[i]));
            }
        }
    }

    int GetProbability(Card card)
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
    }

    public List<Card> SelectCards(int quantity)
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
    }
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

