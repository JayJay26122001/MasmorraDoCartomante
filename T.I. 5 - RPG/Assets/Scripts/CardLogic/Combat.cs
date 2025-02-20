using UnityEngine;

public class Combat : MonoBehaviour
{
    public Creature creature1, creature2;
    public void StartCombat()
    {
        creature1.Enemy = creature2;
        creature2.Enemy = creature1;
    }
}
