using System.Collections.Generic;
using UnityScript.Lang;

public class FixedSizedQueue<T>
{
    Queue<T> q = new Queue<T>();

    public int Limit { get; set; }

    public int GetCount()
    {
        return q.Count;
    }

    public void Enqueue(T obj)
    {
        q.Enqueue(obj);
        
        while (q.Count > Limit)
        {
            q.Dequeue();
        }
    }

    public object Dequeue()
    {
        return q.Dequeue();
    }

    public Array ToArray()
    {
        return q.ToArray();
    }
}
