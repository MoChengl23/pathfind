using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using System;
using System.Linq;
public static class StaticData
{
    public static readonly int GridWidth = 128;
    public static readonly int ClusterWidth = 16;
    public static readonly int HowManyGridsInACluster = ClusterWidth * ClusterWidth;
    public static readonly int ClusterCount = Mathf.CeilToInt(GridWidth / (float)ClusterWidth);
    public static readonly int[] Index8 = new int[3] { 0, 1, -1 };
    public static readonly int2[] Index4 = new int2[4] { new int2(0, 1), new int2(1, 0), new int2(-1, 0), new int2(0, -1) };
    public static bool CheckInField(int2 pos, int range)
    {
        return pos.x >= 0 &&
            pos.x < range &&
            pos.y < range &&
            pos.y >= 0;
    }
    public static bool MyEquals(this int2 a, int2 b)
    {
        var t = a == b;
        return t.x && t.y;
    }
    public static int clusterIndexFlatten(this int2 a)
    {
        return a.x * ClusterWidth + a.y;
    }
    public static T GetItem<T>(this NativeArray<T> grids, int2 index, int width) where T : struct
    {
        if (index.x * width + index.y > 255)
        {
            int a = 1;
            return grids[0];
        }
        return grids[index.x * width + index.y];
    }


    public static T GetItem<T>(this List<T> grids, int2 index, int width)
    {
        return grids[index.x * width + index.y];
    }
    public static void SetItem<T>(this NativeArray<T> grids, int2 index, T item) where T : struct
    {
        grids[index.x * ClusterWidth + index.y] = item;
    }

    public static int2 TransferIndex(int2 bottomLeft)
    {
        var x = bottomLeft.x / ClusterWidth;
        var y = bottomLeft.y / ClusterWidth;
        return new int2(x, y);
    }

    public static T GetMedianItem<T>(this NativeArray<T> grids, int2 from, int2 end, int width) where T : struct
    {
        var sum2 = from + end;

        int2 index = sum2 / 2;

        return grids.GetItem(index, width);
    }
    public static List<T> GetSlice<T>(this NativeArray<T> array, int2 from, int2 end, int2 dir) where T : struct
    {
        var temp = new List<T>();
        int a = 0;
        for (; !from.MyEquals(end); from += dir)
        {

            temp.Add(array.GetItem(from, ClusterWidth));
        }
        temp.Add(array.GetItem(end, ClusterWidth));
        a = 0;
        return temp;
    }
    /// <summary>
    /// one dim of a must be zero, like (x,0) or (0,x) 
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static int ScalarAbs(this int2 a, int2 dim)
    {
        if (dim.x == 0)
        {
            return math.abs(a[1]);
        }
        else
        {
            return math.abs(a[0]);
        }
    }
    public static NativeList<T> ReverseNativeList<T>(this NativeList<T> list) where T : unmanaged
    {
        var newList = new NativeList<T>(Allocator.Persistent);
        for (int i = list.Length - 1; i >= 0; i--)
        {
            newList.Add(list[i]);
        }
        return newList;
    }


    public static NativeList<T> GetNeighbors<T>(this NativeArray<T> array, int2 pos, int range) where T : unmanaged
    {
        var index = Index8;
        var neighbors = new NativeList<T>(Allocator.Persistent);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (i == 0 && j == 0) continue;
                if (CheckInField(pos + new int2(index[i], index[j]), range))
                {
                    neighbors.Add(array.GetItem(pos + new int2(index[i], index[j]), range));
                }
            }
        }
        return neighbors;


    }

    public static List<T> GetClusterNeighbors<T>(this List<T> array, int2 pos, int range)
    {
        var index = Index4;
        var neighbors = new List<T>();
        for (int i = 0; i < 4; i++)
        {





            if (CheckInField(pos + index[i], range))
            {
                neighbors.Add(array.GetItem(pos + index[i], range));
            }
        }
        return neighbors;


    }

    public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
    {
        if (dic.ContainsKey(key)) return;
        dic.Add(key, value);
    }

    public static PortalNodeDir GetDir(int2 d)
    {
        if (d.MyEquals(new int2(1, 0)))
        {
            return PortalNodeDir.Top;
        }
        else if (d.MyEquals(new int2(0, -1)))
        {
            return PortalNodeDir.Right;
        }
        else if (d.MyEquals(new int2(-1, 0)))
        {
            return PortalNodeDir.Bottom;
        }
        else
        {
            return PortalNodeDir.Left;
        }
    }
    public static T Last<T>(this NativeList<T> list) where T : unmanaged
    {
        return list[list.Length - 1];
    }


}
