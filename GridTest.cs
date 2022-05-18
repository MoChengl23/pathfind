using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
public partial class GridTest : SystemBase
{
    private NativeArray<Grid> grids;
    public List<Cluster> clusters = new List<Cluster>();
    protected override void OnCreate()
    {
        grids = GridSystem.grids;
        clusters.AddRange( GridSystem.GenerateClusters());
        


    }
    protected override void OnUpdate()
    {
        
    }
}
