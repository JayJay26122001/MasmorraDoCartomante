using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class ListUT
{
    public static List<T> Shuffle<T>(List<T> list)
    {
        List<T> temp = list.ToList();
        for (int n = temp.Count; n > 1;)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = temp[k];
            temp[k] = temp[n];
            temp[n] = value;
        }
        return temp;
    }
    public static Stack<T> Shuffle<T>(Stack<T> stack)
    {
        List<T> list = stack.ToList();
        stack.Clear();

        for (int n = stack.Count; n > 1;)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return ToStack(list);

    }
    public static Stack<T> ToStack<T>(List<T> list)
    {
        Stack<T> stack = new Stack<T>();
        foreach (T v in list)
        {
            stack.Push(v);
        }
        return stack;
    }
}
