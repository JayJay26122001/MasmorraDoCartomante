using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ModularVar
{
    public enum ValueType { Fixed, Random }
}

[Serializable]
public class ModularInt : ModularVar
{
    [SerializeField] ValueType type;
    //[Header("Fixed")]
    public int value;
    //[Header("Random")]
    public int min;
    public int max;
    public List<ModularIntModifier> modifiers;
    public int GetValue()
    {
        int value = GetBaseValue();
        foreach (ModularIntModifier m in modifiers)
        {
            value = m.ApplyOperation(value);
        }
        return value;

    }
    int GetBaseValue()
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
public class ModularIntModifier
{
    public enum Equations { DividedBy, MultipliedBy, Add, Subdivide }
    public Equations operation;
    public ModularInt value = new ModularInt();
    public int ApplyOperation(int BaseValue)
    {
        switch (operation)
        {
            case Equations.DividedBy:
                return BaseValue / value.GetValue();
            case Equations.MultipliedBy:
                return BaseValue * value.GetValue();
            case Equations.Add:
                return BaseValue + value.GetValue();
            case Equations.Subdivide:
                return BaseValue - value.GetValue();
            default: return 0;
        }
    }
}
[Serializable]
public class ModularFloat : ModularVar
{
    [SerializeField] ValueType type;
    //[Header("Fixed")]
    public float value;
    //[Header("Random")]
    public float min;
    public float max;
    public List<ModularFloatModifier> modifiers;
    public float GetValue()
    {
        float value = GetBaseValue();
        foreach (ModularFloatModifier m in modifiers)
        {
            value = m.ApplyOperation(value);
        }
        return value;

    }
    float GetBaseValue()
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
[Serializable]
public class ModularFloatModifier
{
    public enum Equations { DividedBy, MultipliedBy, Add, Subdivide }
    public Equations operation;
    public ModularFloat value = new ModularFloat();
    public float ApplyOperation(float BaseValue)
    {
        switch (operation)
        {
            case Equations.DividedBy:
                return BaseValue / value.GetValue();
            case Equations.MultipliedBy:
                return BaseValue * value.GetValue();
            case Equations.Add:
                return BaseValue + value.GetValue();
            case Equations.Subdivide:
                return BaseValue - value.GetValue();
            default: return 0;
        }
    }
}