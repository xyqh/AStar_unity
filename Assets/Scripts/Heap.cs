using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heap<T> where T : IHeapItem<T> {
    public delegate bool comp(T a, T b);
    public int Count
    {
        get
        {
            return array.Count;
        }
    }

    void swap(T a, T b)
    {
        array[a.HeapIndex] = b;
        array[b.HeapIndex] = a;
        a.HeapIndex += b.HeapIndex;
        b.HeapIndex = a.HeapIndex - b.HeapIndex;
        a.HeapIndex = a.HeapIndex - b.HeapIndex;
    }

    public List<T> array;

    void Heap_Up(T t)
    {
        int cur = t.HeapIndex;
        int pIdx = (cur - 1) / 2;
        // 假如fun是greater，那就是小顶堆，那么当前比父节点小的话就需要上浮了
        while (cur >= 0 && pIdx >= 0 && array[cur].CompareTo(array[pIdx]) > 0)
        {
            swap(array[cur], array[pIdx]);

            cur = pIdx;
            pIdx = (cur - 1) / 2;

            t.HeapIndex = cur;
        }
    }

    void Heap_Down(T t)
    {
        int cur = t.HeapIndex, lChild = 2 * cur + 1, rChild = 2 * cur + 2;
        // 假如fun是greater，那就是小顶堆，那么当前两个子节点其中有一个更小的话就需要下沉了
        while(cur < Count && ((lChild < Count && array[lChild].CompareTo(array[cur]) > 0) || (rChild < Count && array[rChild].CompareTo(array[cur]) > 0)))
        {
            // 取其中更小的那个
            int cIdx = lChild;
            if(rChild < Count && array[rChild].CompareTo(array[lChild]) > 0)
            {
                cIdx = rChild;
            }
            
            swap(array[cur], array[cIdx]);

            cur = cIdx;
            lChild = 2 * cur + 1;
            rChild = 2 * cur + 2;
        }
    }

    //bool Heap_Check()
    //{
    //    if (array.Count < 1) return true;
    //    Queue<int> que = new Queue<int>();
    //    que.Enqueue(0);
    //    while(que.Count > 0)
    //    {
    //        int idx = que.Dequeue();
    //        int lChild = 2 * idx + 1;
    //        if(lChild < array.Count)
    //        {
    //            if(fun(array[idx], array[lChild]))
    //            {
    //                return false;
    //            }
    //            que.Enqueue(lChild);
    //        }
    //        int rChild = 2 * idx + 2;
    //        if (rChild < array.Count)
    //        {
    //            if (fun(array[idx], array[rChild]))
    //            {
    //                return false;
    //            }
    //            que.Enqueue(rChild);
    //        }
    //    }

    //    return true;
    //}

    public void Add(T t)
    {
        array.Add(t);
        t.HeapIndex = array.Count - 1;
        Heap_Up(t);
    }

    public void Pop()
    {
        int n = array.Count;
        if (n < 1) return;
        swap(array[n - 1], array[0]);
        array.RemoveAt(n - 1);
        if(array.Count > 0) Heap_Down(array[0]);
    }

    public T Top()
    {
        return array[0];
    }

    public void UpdateItem(T t)
    {
        Heap_Up(t);
    }

    public Heap()
    {
        array = new List<T>();
    }

    public override string ToString()
    {
        string str1 = "";
        for(int i = 0; i < array.Count; ++i)
        {
            str1 += array[i].ToString() + " ";
        }
        return str1;
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}