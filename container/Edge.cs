using System.Threading;
using System.Reflection.Emit;

using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System;
 
using Unity.Jobs;
using Unity.Profiling;

public struct EdgeTemp : IEquatable<EdgeTemp>
{
    public int2 fromPos;
    public int2 endPos;
    public bool Equals(EdgeTemp other)
    {


        return (fromPos.MyEquals(other.fromPos) && endPos.MyEquals(other.endPos))
            || (fromPos.MyEquals(other.endPos) && endPos.MyEquals(other.fromPos));

    }
    public override int GetHashCode()
    {
        var dir = (endPos - fromPos).clusterIndexFlatten();
        var code = fromPos.clusterIndexFlatten() * StaticData.ClusterWidth * StaticData.ClusterWidth + math.abs(dir);

        return fromPos.clusterIndexFlatten() * StaticData.ClusterWidth * StaticData.ClusterWidth + math.abs(dir);
    }

}


public class Edge : IEquatable<Edge>
{

    public Grid fromGrid;

    public Grid endGrid;

    public int2 from { get => fromGrid.worldPos; }

    public int2 end { get => endGrid.worldPos; }
    public PortalNode fromNode;
    public PortalNode endNode;


    public NativeList<int2> path;

    // public static int StaticData.ClusterWidth = StaticData.ClusterWidth;

    public bool done;



    public int weight;


    /// <summary>
    /// only for connect cluster Border
    /// </summary>
    /// <param name="fromNode"></param>
    /// <param name="endNode"></param>
    /// <param name="weight"></param>
    /// <param name="outputGrid"></param>
    /// <param name="inputGrid"></param>
    public Edge(PortalNode fromNode, PortalNode endNode, int weight, Grid outputGrid, Grid inputGrid)
    {
        this.fromNode = fromNode;
        this.endNode = endNode;
        this.weight = weight;
        this.fromGrid = outputGrid;
        this.endGrid = inputGrid;


    }


    public Edge(PortalNode from, PortalNode end)
    {
        this.fromNode = from;
        this.endNode = end;
    }


    /// <summary>
    /// 无参数的构造函数，用于对象池的泛型构造
    /// </summary>
    public Edge(){

    }

    public void Init(PortalNode fromNode, PortalNode endNode, int weight, Grid outputGrid, Grid inputGrid)
    {
        this.fromNode = fromNode;
        this.endNode = endNode;
        this.weight = weight;
        this.fromGrid = outputGrid;
        this.endGrid = inputGrid;
    }
    public void Init(PortalNode from, PortalNode end)
    {
        this.fromNode = from;
        this.endNode = end;
    }
    public void Init(){

    }



    public override int GetHashCode()
    {
        return from.clusterIndexFlatten() * StaticData.HowManyGridsInACluster + end.clusterIndexFlatten();

    }
    public void ReleaseMemory()
    {
        if (path.IsCreated)
        {
            NativeAllocatePool.GiveBackToPool(path, Allocator.TempJob);
        }
    }

    public JobHandle BuildPath(Grid fromGrid, Grid endGrid, NativeArray<Grid> grids)
    {

        var c = new ProfilerMarker("Allocate");
        // var d = new ProfilerMarker();
        c.Begin();
        // path = NativeAllocatePool.PullNativeList<int2>(Allocator.TempJob);
        if (path.IsCreated)
        {
            Debug.Log("already exist path");
            NativeAllocatePool.GiveBackToPool(path, Allocator.TempJob);
        }
        // path = new NativeList<int2>(Allocator.TempJob);
        path = NativeAllocatePool.PullNativeList<int2>(Allocator.TempJob);
        c.End();


        JobHandle jobHandle = new FindPathJob
        {
            fromGrid = fromGrid,
            endGrid = endGrid,
            grids = grids,
            path = path
        }.Schedule();

        return jobHandle;

    }
    [BurstCompile]
    struct FindPathJob : IJob
    {
        public Grid fromGrid;
        
        public Grid endGrid;
        public NativeList<int2> path;

        [ReadOnly]
        public NativeArray<Grid> grids;
        public void Execute()
        {
            NativeArray<Grid> tempGrids = new NativeArray<Grid>(grids, Allocator.Temp);
            path = FindPath(tempGrids);



        }
        private NativeList<int2> FindPath(NativeArray<Grid> tempGrids)
        {

            var start = fromGrid;
            Heap<Grid> heap = new Heap<Grid>(StaticData.ClusterWidth * StaticData.ClusterWidth, true);
            start.gcost = 1;
            start.hcost = CalculateDistanceCost(start.worldPos, endGrid.worldPos);
            start.fcost = start.hcost + 1;
            heap.Push(start);

            NativeHashSet<int2> visited = new NativeHashSet<int2>(StaticData.ClusterWidth * StaticData.ClusterWidth, Allocator.Temp);
            visited.Add(fromGrid.worldPos);
            while (!heap.IsEmpty)
            {
                var front = heap.Pop();


                if (front.Equals(endGrid))
                    break;
                visited.Add(front.worldPos);
                // NativeList<int2> neighbors = new NativeList<int2>(Allocator.Temp);
                NativeList<Grid> neighbors = tempGrids.GetNeighbors(front.localPos, StaticData.ClusterWidth, true);
                int a = 1;
                for (int i = 0; i < neighbors.Length; i++)
                {
                    int2 neighborLocalPos = neighbors[i].localPos;
                    int2 neighborWorldPos = neighbors[i].worldPos;
                    int aa = 2;
                    // var temp = tempGrids.GetItem(neighborIndex);
                    // var temp2 = visited.Contains(neighborIndex);

                    if (visited.Contains(neighborWorldPos) || !tempGrids.GetItem(neighborLocalPos, StaticData.ClusterWidth).isWalkable)
                        continue;

                    var neighborGrid = tempGrids.GetItem(neighborLocalPos, StaticData.ClusterWidth);

                    int tempGCost = front.gcost + CalculateDistanceCost(front.worldPos, neighborWorldPos);
                    if (tempGCost < neighborGrid.gcost)
                    {
                        neighborGrid.camefrom = front.localPos;
                        neighborGrid.gcost = tempGCost;
                        neighborGrid.hcost = CalculateDistanceCost(neighborWorldPos, endGrid.worldPos);
                        neighborGrid.fcost = neighborGrid.gcost + neighborGrid.hcost;

                        tempGrids.SetItem(neighborLocalPos, neighborGrid);
                        if (!heap.Contains(neighborGrid))
                        {
                            heap.Push(neighborGrid);
                        }
                    }

                }
                neighbors.Dispose();

            }
            visited.Dispose();
            heap.Release();
            var res = GetPath(tempGrids, fromGrid, endGrid);



            return res;


        }

        private NativeList<int2> GetPath(NativeArray<Grid> tempGrids, Grid from, Grid end)
        {
            // NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            var cur = tempGrids.GetItem(end.localPos, StaticData.ClusterWidth);

            while (cur.camefrom.x != -1)
            {
                path.Add(cur.worldPos);
                cur = tempGrids.GetItem(cur.camefrom, StaticData.ClusterWidth);
            }
            return path;
        }

        private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            // return xDistance + yDistance;

            return 14 * math.min(xDistance, yDistance) + 10 * remaining;

        }


    }
    public bool Equals(Edge other)
    {
        return (fromNode == other.fromNode && endNode == other.endNode)
         || (fromNode == other.endNode && endNode == other.fromNode);
    }


}

