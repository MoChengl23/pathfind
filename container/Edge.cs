using System.Threading;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Object = System.Object;
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
    public ManualResetEvent doneEvent;
    public static int ClusterWidth = StaticData.ClusterWidth;

    public bool done;



    public int weight;

    /// <summary>
    /// for connect internal portalnode
    /// </summary>
    /// <param name="fromNode"></param>
    /// <param name="endNode"></param>
    /// <param name="d"></param>
    public Edge(PortalNode fromNode, PortalNode endNode, ManualResetEvent d)
    {
        this.fromNode = fromNode;
        this.endNode = endNode;
        doneEvent = d;
        doneEvent.Reset();

    }
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

    
    
    
    /// <summary>
    /// for connect temp PortalNode to cluster borderNode
    /// </summary>
    /// <param name="from"></param>
    /// <param name="end"></param>
    public Edge(PortalNode from, PortalNode end){
        this.fromNode = from;
        this.endNode = end;
    }
    
    
    public override int GetHashCode()
    {
        return from.clusterIndexFlatten() * StaticData.HowManyGridsInACluster + end.clusterIndexFlatten();

    }
    public void BuildPath(Object o)
    {
        object[] os = o as object[];

        Grid fromGrid = (Grid)os[0];
        Grid endGrid = (Grid)os[1];
        NativeArray<Grid> grids = (NativeArray<Grid>)os[2];

        NativeArray<Grid> tempGrids = new NativeArray<Grid>(grids, Allocator.Persistent);

        path = FindPath(tempGrids, fromGrid, endGrid);

        doneEvent.Set();

    }
    private NativeList<int2> FindPath(NativeArray<Grid> tempGrids, Grid from, Grid end)
    {

        var start = from;
        Heap<Grid> heap = new Heap<Grid>(StaticData.ClusterWidth * StaticData.ClusterWidth);
        start.gcost = 1;
        start.hcost = CalculateDistanceCost(start.worldPos, end.worldPos);
        start.fcost = start.hcost + 1;
        heap.Push(start);

        NativeHashSet<int2> visited = new NativeHashSet<int2>(StaticData.ClusterWidth * StaticData.ClusterWidth, Allocator.Persistent);

        while (!heap.IsEmpty)
        {
            var front = heap.Pop();

            if (front.Equals(end))
                break;
            visited.Add(front.worldPos);
            // NativeList<int2> neighbors = new NativeList<int2>(Allocator.Temp);
            NativeList<Grid> neighbors = tempGrids.GetNeighbors(front.worldPos, StaticData.ClusterWidth);

            for (int i = 0; i < neighbors.Length; i++)

            {
                int2 neighborIndex = neighbors[i].worldPos;
                // var temp = tempGrids.GetItem(neighborIndex);
                // var temp2 = visited.Contains(neighborIndex);

                if (visited.Contains(neighborIndex) || !tempGrids.GetItem(neighborIndex, ClusterWidth).isWalkable)
                    continue;

                var neighborGrid = tempGrids.GetItem(neighborIndex, ClusterWidth);

                int tempGCost = front.gcost + CalculateDistanceCost(front.worldPos, neighborIndex);
                if (tempGCost < neighborGrid.gcost)
                {
                    neighborGrid.camefrom = front.localPos;
                    neighborGrid.gcost = tempGCost;
                    neighborGrid.hcost = CalculateDistanceCost(neighborIndex, end.worldPos);
                    neighborGrid.fcost = neighborGrid.gcost + neighborGrid.hcost;

                    tempGrids.SetItem(neighborIndex, neighborGrid);
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
        var res = GetPath(tempGrids, from, end);



        return res;


    }

    private NativeList<int2> GetPath(NativeArray<Grid> tempGrids, Grid from, Grid end)
    {
        NativeList<int2> path = new NativeList<int2>(Allocator.Persistent);
        var cur = tempGrids.GetItem(end.localPos, ClusterWidth);

        while (cur.camefrom.x != -1)
        {
            path.Add(cur.worldPos);
            cur = tempGrids.GetItem(cur.camefrom, ClusterWidth);
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

    public bool Equals(Edge other)
    {
        return (fromNode == other.fromNode && endNode == other.endNode)
         || (fromNode == other.endNode && endNode == other.fromNode);
    }
}

