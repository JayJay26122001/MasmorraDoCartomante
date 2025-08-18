using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatModifier
{
    public enum ModfierType { Multiply, Add }
    public ModfierType type;
    public float value;
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
                res = stat * value;
                break;
            case ModfierType.Add:
                res = stat + value;
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
                res = (int)(stat * value);
                break;
            case ModfierType.Add:
                res = stat + (int)value;
                break;
        }
        return res;
    }
}
