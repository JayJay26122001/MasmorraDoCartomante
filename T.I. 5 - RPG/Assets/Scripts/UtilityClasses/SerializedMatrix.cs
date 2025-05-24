using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using System;

[Serializable]
public class SerializedMatrix<T>
{
    public List<Envelope<T>> matrix = new List<Envelope<T>>();
    [SerializeField]int xLength, yLength;
    public int XLength
    {
        get { return xLength; }
    }
    public int YLength
    {
        get { return yLength; }
    }

    public SerializedMatrix(int x, int y)
    {
        xLength = x;
        yLength = y;
        for (int i = 0; i < x * y; i++)
        {
            matrix.Insert(i, new Envelope<T>());
        }
    }

    public Envelope<T> GetValue(int x, int y)
    {
        return matrix[y * xLength + x];
    }
}

[Serializable]
public class Envelope<T>
{
    public T value;
}
