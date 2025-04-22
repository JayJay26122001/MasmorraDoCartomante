using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SerializableStack<T>
{
    [SerializeField] protected List<T> Stack = new List<T>();
    public int Count
    {
        get { return Stack.Count; }
    }
    public T GetVar(int index)
    {
        index = math.clamp(index, 0, Stack.Count - 1);
        return Stack[index];
    }
    public void Add(T value)
    {
        Stack.Insert(0, value);
    }
    public T GetTop()
    {
        T temp = Stack[0];
        Remove(Stack[0]);
        return temp;
    }
    public void Remove(T value)
    {
        Stack.Remove(value);
    }
    public void Remove(int index)
    {
        Stack.Remove(Stack[index]);
    }
    public void Clear()
    {
        Stack.Clear();
    }
    public List<T> ToList()
    {
        return Stack.ToList();
    }
}
