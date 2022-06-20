using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using System.Threading;





public static class NativeAllocatePool
{
    // public static implicit operator MyAllocator() 

    public static NativeList<T> PullNativeList<T>(Allocator allocator) where T : unmanaged
    {
        return NativeAllocatePool<T>.PullNativeList(allocator);
    }
    public static void GiveBackToPool<T>(NativeList<T> list, Allocator allocator) where T : unmanaged
    {
        NativeAllocatePool<T>.GiveBackToPool(list, allocator);
    }

}

public static class NativeAllocatePool<T> where T : unmanaged
{

    public static ConcurrentBag<NativeList<T>> tempJobPool = new ConcurrentBag<NativeList<T>>();

    public static ConcurrentBag<NativeList<T>> permanentPool = new ConcurrentBag<NativeList<T>>();

    public static NativeList<T> PullNativeList(Allocator allocator)
    {
       
        switch (allocator)
        {

            case Allocator.TempJob:
                if(tempJobPool.TryTake(out  NativeList<T> res)){
                    return res;
                }
                break;
            case Allocator.Persistent:
                if(permanentPool.TryTake(out NativeList<T> res1)){
                    return res1;
                }
                break;
            default:
                return new NativeList<T>(allocator);

        }
        return default(NativeList<T>);

    }
    public static void Init()
    {
        for (int i = 0; i < 100; i++)
        {
            tempJobPool.Add(new NativeList<T>(Allocator.TempJob));
        }
    }

    public static void GiveBackToPool(NativeList<T> list, Allocator allocator)
    {
        list.Clear();
        switch (allocator)
        {

            case Allocator.TempJob:
                tempJobPool.Add(list);
                break;
            case Allocator.Persistent:
                permanentPool.Add(list);
                break;
        }

    }
    public static void DisPlay()
    {
        Debug.Log(permanentPool.Count);
    }




}



public static class AllocatePool
{
    public static T PullItem<T>() where T : new()
    {
        return AllocatePool<T>.PullItem();
    }
    public static void GiveBackToPool<T>(T item) where T : new()
    {
        AllocatePool<T>.GiveBackToPool(item);
    }
}
public static class AllocatePool<T> where T : new()
{
    public static ConcurrentBag<T> pool = new ConcurrentBag<T>();
    /// <summary>
    /// pulled Item is dirty, need to be Inited first
    /// </summary>
    /// <returns></returns>
    public static T PullItem()
    {
        T res;
        pool.TryTake(out res);
        if (res == null)
        {
            res = new T();
        }
        return res;

    }
    public static void Init()
    {
        for (int i = 0; i < 100; i++)
        {
            pool.Add(new T());
        }
    }

    public static void GiveBackToPool(T item)
    {
        pool.Add(item);
    }
}