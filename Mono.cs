using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Profiling;
using Unity.Mathematics;

public class Mono : MonoBehaviour
{
    Cluster cluster0, cluster1;
    public int2 fromPos, endPos;

    void Start()
    {
        cluster0 = World.DefaultGameObjectInjectionWorld.GetExistingSystem<GridTest>().clusters[0];
        cluster1 = World.DefaultGameObjectInjectionWorld.GetExistingSystem<GridTest>().clusters[1];

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                var pos = hitInfo.point;
                fromPos = new int2((int)pos.x, (int)pos.z);
                


            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                var pos = hitInfo.point;
                endPos = new int2((int)pos.x, (int)pos.z);
                 


            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NativeAllocatePool<int2>.Init();
            AllocatePool<Edge>.Init();
            Profiler.BeginSample("mytime");
            PathFindUtil.FindPath(new int2(4,3), new int2(55,33));
            Profiler.EndSample();
        }
    }
    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        for (int i = 0; i < 128; i++)
        {
            for (int j = 0; j < 128; j++)
            {
                if (cluster0 == null) return;
                if (cluster0.grids.GetItem(new int2(i, j), StaticData.ClusterWidth).isWalkable)
                {
                    Gizmos.DrawWireCube(new Vector3(i, 0, j), new Vector3(1, 0, 1));
                    // Debug.Log("iswalkable");
                }

                else
                {
                    Gizmos.DrawCube(new Vector3(i, 0, j), new Vector3(1, 0, 1));

                }
            }
        }

        for (int i = 0; i < cluster0.portalNodes.Count; i++)
        {
            for (int j = i + 1; j < cluster0.portalNodes.Count; j++)
            {
                var a = cluster0.portalNodes[i];
                var b = cluster0.portalNodes[j];
                if (a.Collinear(b)) continue;


                Gizmos.DrawLine(new Vector3(a.pos.x, 0, a.pos.y), new Vector3(b.pos.x, 0, b.pos.y));
            }
        }
    }
}
