using System;
using UnityEngine;

[Serializable]
public abstract class ModularVar
{
    protected enum ValueType { Fixed, Random }
}

[Serializable]
public class ModularInt : ModularVar
{
    [SerializeField] ValueType type;
    [Header("Fixed")]
    public int value;
    [Header("Random")]
    public int min, max;
    public int GetValue()
    {
        switch (type)
        {
            case ValueType.Fixed:
                return value;
            case ValueType.Random:
                return UnityEngine.Random.Range(min, max + 1);
            default: return 0;
        }
    }


}
[Serializable]
public class ModularFloat : ModularVar
{
    [SerializeField] ValueType type;
    [Header("Fixed")]
    public float value;
    [Header("Random")]
    public float min, max;
    public float GetValue()
    {
        switch (type)
        {
            case ValueType.Fixed:
                return value;
            case ValueType.Random:
                return UnityEngine.Random.Range(min, max);
            default: return 0;
        }
    }
}