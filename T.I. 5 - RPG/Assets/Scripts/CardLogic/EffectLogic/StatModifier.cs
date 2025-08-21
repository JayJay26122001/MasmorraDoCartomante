using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatModifier
{
    public enum ModfierType { Multiply, Add }
    public ModfierType type;
    public ModularFloat value;
    public static float ApplyModfierList(float stat, List<StatModifier> modifiers)
    {
        float res = stat;
        foreach (StatModifier m in modifiers)
        {
            res = m.ApplyModfier(res);
        }
        return res;
    }
    public static int ApplyModfierList(int stat, List<StatModifier> modifiers)
    {
        int res = stat;
        foreach (StatModifier m in modifiers)
        {
            res = m.ApplyModfier(res);
        }
        return res;
    }
    public float ApplyModfier(float stat)
    {
        float res = stat;
        switch (type)
        {
            case ModfierType.Multiply:
                res = stat * value.GetValue();
                break;
            case ModfierType.Add:
                res = stat + value.GetValue();
                break;
        }
        return res;
    }
    public int ApplyModfier(int stat)
    {
        int res = stat;
        switch (type)
        {
            case ModfierType.Multiply:
                res = (int)(stat * value.GetValue());
                break;
            case ModfierType.Add:
                res = stat + (int)value.GetValue();
                break;
        }
        return res;
    }
}
