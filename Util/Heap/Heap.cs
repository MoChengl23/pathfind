
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


public struct TestNode : IComparable<TestNode>, IEquatable<TestNode>
{
    public int w;
    public int CompareTo(TestNode obj)
    {
        if (w == 0) return 1;
        if (obj.w == 0) return -1;
        return w - obj.w;
    }



    public bool Equals(TestNode other)
    {
        return w == other.w;
    }
    public override string ToString()
    {
        return w.ToString();
    }
}


public struct Heap<T> where T : unmanaged, IComparable<T>, IEquatable<T>
{

    [NativeDisableContainerSafetyRestriction]
    public NativeList<T> heaptree;

    [NativeDisableContainerSafetyRestriction]
    public NativeHashSet<T> heapset;
    public Heap(int capcity)
    {
        this.heaptree = new NativeList<T>(Allocator.Persistent);
        this.heapset = new NativeHashSet<T>(capcity, Allocator.Persistent);
    }

    public bool IsEmpty
    {
        get
        {
            return heaptree.Length == 0;
        }
    }
    public int Length
    {
        get
        {
            return heaptree.Length;
        }
    }

    public T Pop()
    {
        var res = heaptree[0];
        heapset.Remove(res);
        heaptree.RemoveAtSwapBack(0);
        AdjustDown(0);
        return res;
    }
    public void Push(T newNode)
    {
        heaptree.Add(newNode);
        heapset.Add(newNode);
        AdjustUp();
    }
    public void Display()
    {
        string a = string.Empty;
        foreach (var i in heaptree)
        {
            a += i.ToString();
            a += "  ";
        }


    }




    private void AdjustUp()
    {
        var temp = heaptree[heaptree.Length - 1];
        var tempIndex = heaptree.Length - 1;
        var fatherIndex = tempIndex / 2;

        while (tempIndex > 0 && temp.CompareTo(heaptree[fatherIndex]) < 0)
        {

            Swap(fatherIndex, tempIndex);
            tempIndex = fatherIndex;
            fatherIndex /= 2;

        }

    }

    private void AdjustDown(int index)
    {

        while (GetSmallerChild(index, out T child, out int childIndex) && heaptree[index].CompareTo(heaptree[childIndex]) > 0)
        {
            Swap(index, childIndex);
            index = childIndex;
        }

    }

    private bool GetSmallerChild(int fatherIndex, out T child, out int childIndex)
    {
        T childA = default(T);
        T childB = default(T);
        int childAIndex = fatherIndex * 2 + 1;
        int childBIndex = fatherIndex * 2 + 2;



        if (childAIndex >= heaptree.Length && childBIndex >= heaptree.Length)
        {
            child = default(T);
            childIndex = -1;
            return false;
        }



        if (childAIndex < heaptree.Length)
            childA = heaptree[childAIndex];
        if (childBIndex < heaptree.Length)
            childB = heaptree[childBIndex];




        if (childA.CompareTo(childB) < 0)
        {
            child = childA;
            childIndex = childAIndex;
        }
        else
        {
            child = childB;
            childIndex = childBIndex;
        }
        return true;



    }
    public bool Contains(T node)
    {
        return heapset.Contains(node);
    }



    private void Swap(int indexA, int indexB)
    {
        var temp = heaptree[indexA];
        heaptree[indexA] = heaptree[indexB];
        heaptree[indexB] = temp;
    }
    public void Release()
    {
        if (heapset.IsCreated)
            heapset.Dispose();
        if (heaptree.IsCreated)
            heaptree.Dispose();
    }



}

public class ClassHeap<T> where T : IEquatable<T>
{

    /// <summary>
    /// class | priority
    /// </summary>
    public List<(T, int)> heaptree;


    public HashSet<T> heapset;
    public ClassHeap()
    {
        this.heaptree = new List<(T, int)>();
        this.heapset = new HashSet<T>();
    }

    public bool IsEmpty
    {
        get
        {
            return heaptree.Count == 0;
        }
    }
    public int Length
    {
        get
        {
            return heaptree.Count;
        }
    }

    public T Pop()
    {
        var (res, p) = heaptree[0];
        heapset.Remove(res);
        heaptree.RemoveAtSwapBack(0);
        AdjustDown(0);
        return res;
    }

    public void Push(T newNode, int priority)
    {
        heaptree.Add((newNode, priority));
        heapset.Add(newNode);
        AdjustUp();
    }

    public void Display()
    {
        string a = string.Empty;
        foreach (var (j, i) in heaptree)
        {
            a += i.ToString();
            a += "  ";
        }
        Debug.Log(a);


    }




    private void AdjustUp()
    {
        var (tempclass, priority) = heaptree[heaptree.Count - 1];
        var tempIndex = heaptree.Count - 1;
        var fatherIndex = tempIndex / 2;

        while (tempIndex > 0 && priority < heaptree[fatherIndex].Item2)
        {

            Swap(fatherIndex, tempIndex);
            tempIndex = fatherIndex;
            fatherIndex /= 2;

        }

    }


    private void AdjustDown(int index)
    {

        while (GetSmallerChild(index, out T child, out int childIndex)
            && heaptree[index].Item2 > heaptree[childIndex].Item2)
        {

            Swap(index, childIndex);
            index = childIndex;
        }

    }

    private bool GetSmallerChild(int fatherIndex, out T child, out int childIndex)
    {
        T childA = default(T);
        T childB = default(T);
        int priorityA = int.MaxValue, priorityB = int.MaxValue;
        int childAIndex = fatherIndex * 2 + 1;
        int childBIndex = fatherIndex * 2 + 2;



        if (childAIndex >= heaptree.Count && childBIndex >= heaptree.Count)
        {
            child = default(T);
            childIndex = -1;
            return false;
        }



        if (childAIndex < heaptree.Count)
            (childA, priorityA) = heaptree[childAIndex];
        if (childBIndex < heaptree.Count)
            (childB, priorityB) = heaptree[childBIndex];




        if (priorityA < priorityB)
        {
            child = childA;
            childIndex = childAIndex;
        }
        else
        {
            child = childB;
            childIndex = childBIndex;
        }

        return true;



    }
    public bool Contains(T node)
    {
        return heapset.Contains(node);
    }



    private void Swap(int indexA, int indexB)
    {
        var temp = heaptree[indexA];
        heaptree[indexA] = heaptree[indexB];
        heaptree[indexB] = temp;
    }



}



