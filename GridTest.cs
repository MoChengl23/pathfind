using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.Profiling;

public partial class GridTest : SystemBase
{
    private NativeArray<Grid> grids;
    public List<Cluster> clusters = new List<Cluster>();
    protected override void OnStartRunning()
    {
        grids = GridSystem.grids;
        //    Profiler.BeginSample("MyTest");
        clusters.AddRange( GridSystem.GenerateClusters());
        // Profiler.EndSample();


    }
    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Profiler.BeginSample("MyTest");
            // clusters.AddRange(GridSystem.GenerateClusters());
            Profiler.EndSample();

        }

    }
}
