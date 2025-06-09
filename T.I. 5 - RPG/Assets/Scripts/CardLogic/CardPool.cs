using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardPool", menuName = "CardLogic/CardPool")]
public class CardPool : ScriptableObject
{
    public List<Card> pool = new List<Card>();
    public List<int> probabilities = new List<int>();

    public List<Card> SelectCards(int quantity)
    {
        for(int i = 0; i < pool.Count; i++)
        {
            switch(pool[i].Rarity)
            {
                case Card.CardRarity.Common:
                    probabilities[i] = 30;
                    break;
                    
                case Card.CardRarity.Uncommon:
                    probabilities[i] = 20;
                    break;
                    
                case Card.CardRarity.Rare:
                    probabilities[i] = 10;
                    break;
                    
                case Card.CardRarity.Epic:
                    probabilities[i] = 5;
                    break;
                    
                case Card.CardRarity.Legendary:
                    probabilities[i] = 1;
                    break;
            }
        }
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
                int rand = Random.Range(0, sum);
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
