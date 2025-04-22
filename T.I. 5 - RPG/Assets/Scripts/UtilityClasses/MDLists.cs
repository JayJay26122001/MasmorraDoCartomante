using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MDLists<T>
{
    public T this[int key]
    {
        get
        {
            return InnerList[key];
        }
        set
        {
            InnerList[key] = value;
        }
    }
    public int Count { get { return InnerList.Count; } }
    [SerializeField] List<T> InnerList = new List<T>();
}
