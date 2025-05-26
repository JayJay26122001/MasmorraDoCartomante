using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "PackPool", menuName = "CardLogic/PackPool")]
public class PackPool : ScriptableObject
{
    public List<CardPackSO> pool = new List<CardPackSO>();
    public List<int> probabilities = new List<int>();

    public List<CardPackSO> SelectPacks(int quantity)
    {
        List<CardPackSO> auxList = new List<CardPackSO>();
        int sum = 0;
        foreach (int s in probabilities)
        {
            sum += s;
        }
        while (auxList.Count < quantity)
        {
            bool success = false;
            while (!success)
            {
                int rand = Random.Range(0, sum);
                int aux = 0;
                for (int i = 0; i < probabilities.Count; i++)
                {
                    aux += probabilities[i];
                    if (rand < aux)
                    {
                        if (!auxList.Contains(pool[i]))
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
