using System;
using UnityEngine;

[Serializable]
public class StatModifier
{
    public enum ModfierType { Multiply, Add }
    public ModfierType type;
    public float value;
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
