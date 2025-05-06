using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyPool", menuName = "CardLogic/EnemyPool")]
public class EnemyPool : ScriptableObject
{
    public List<int> pool = new List<int>();
    public List<int> probabilities = new List<int>();

    public int SelectIndex()
    {
        int index = 0;
        int sum = 0;
        foreach (int s in probabilities)
        {
            sum += s;
        }
        int rand = Random.Range(0, sum);
        int aux = 0;
        for (int i = 0; i < probabilities.Count; i++)
        {
            aux += probabilities[i];
            if (rand < aux)
            {
                index = pool[i];
                i = probabilities.Count;
            }
        }
        return index;
    }
}
