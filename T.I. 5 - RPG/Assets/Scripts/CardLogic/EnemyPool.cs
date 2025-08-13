using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyPool", menuName = "CardLogic/EnemyPool")]
public class EnemyPool : ScriptableObject
{
    public List<int> pool = new List<int>();
    public List<ControlledProbability> baseProbabilities = new List<ControlledProbability>();
    List<ControlledProbability> probabilities = new List<ControlledProbability>();

    public void SetupProbabilities()
    {
        probabilities.Clear();
        foreach(ControlledProbability p in baseProbabilities)
        {
            probabilities.Add(new ControlledProbability(p.type, p.probability, p.multiplier, p.minMult, p.maxMult));
        }
    }

    public int SelectIndex()
    {
        int index = 0;
        int sum = 0;
        foreach (ControlledProbability s in probabilities)
        {
            sum += s.probability;
        }
        int rand = Random.Range(0, sum);
        int aux = 0;
        for (int i = 0; i < probabilities.Count; i++)
        {
            aux += probabilities[i].probability;
            if (rand < aux)
            {
                index = pool[i];
                i = probabilities.Count;
            }
        }
        return index;
    }

    public void ModifyMultipliers(string s)
    {
        foreach(ControlledProbability p in probabilities)
        {
            if(string.Compare(s, p.type) == 0)
            {
                p.ModifyMultiplier(-0.5f);
            }
            else
            {
                p.ModifyMultiplier(0.25f);
            }
            p.ModifyProbability(p.probability);
        }
    }
}
