using System.Globalization;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
// using Unity.Profiling;
using UnityEngine.Profiling;

public static class GridSystem
{
    public static NativeArray<Grid> grids = new NativeArray<Grid>(512 * 512, Allocator.Persistent);
    public static List<Cluster> clusters = new List<Cluster>();
    public static List<Cluster> GenerateClusters()
    {
        Profiler.BeginSample("Create Clusters");

        clusters = new List<Cluster>();
        for (int i = 0; i < StaticData.GridWidth; i += StaticData.ClusterWidth)
        {
            for (int j = 0; j < StaticData.GridWidth; j += StaticData.ClusterWidth)
            {
                var cluster = new Cluster(new int2(i, j), new int2(i + StaticData.ClusterWidth - 1, j + StaticData.ClusterWidth - 1));

                clusters.Add(cluster);
            }
        }
        Profiler.EndSample();
        Profiler.BeginSample("Add clusterNeighbors");
        var count = clusters.Count;

        for (int i = 0; i < count; i++)
        {
            var neighbors = clusters.GetClusterNeighbors(clusters[i].clusterPos, (int)math.ceil(math.sqrt(count)));
            clusters[i].AddClusterNeighbor(neighbors);
 

        }
        Profiler.EndSample();
        Profiler.BeginSample("Create PortalNode");
        //to do, make it multiThreading
        for (int i = 0; i < count; i++)
        {
            clusters[i].GeneratePortalNode();
        }
        Profiler.EndSample();
        Profiler.BeginSample("Create Edge");
        for (int i = 0; i < count; i++)
        {
            clusters[i].GenerateEdges();
        }
        Profiler.EndSample();

        return clusters;



    }

    public static void GetClusterAndGrid(int2 pos, out Cluster cluster, out Grid grid)
    {
        int cluster_x = pos.x / StaticData.ClusterWidth;

        int cluster_y = pos.y / StaticData.ClusterWidth;


        int grid_x = pos.x % StaticData.ClusterWidth;
        int grid_y = pos.y % StaticData.ClusterWidth;

     
        cluster = clusters.GetItem(new int2(cluster_x, cluster_y), StaticData.ClusterCount);
        grid = cluster.grids.GetItem(new int2(grid_x, grid_y), StaticData.ClusterWidth);
 


    }










}
