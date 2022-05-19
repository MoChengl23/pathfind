using System.Threading;
using System.Collections.Concurrent;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System;
using Unity.Jobs;
using Unity.Burst;
using System.Linq;
using Object = System.Object;
public enum ClusterDir
{
    Top = 2,
    Bottom = 1,
    Left = 0,
    Right = 3
}

public class Cluster
{

    public static readonly int ClusterWidth = StaticData.ClusterWidth;

    public void TestHit(int2 index)
    {
        var temp = grids.GetItem(index, StaticData.ClusterWidth);
        temp.isWalkable = false;
        grids.SetItem(index, temp);
    }



    public List<PortalNode> portalNodes;

    public List<ManualResetEvent> calculatePath = new List<ManualResetEvent>();

    public IEnumerable<(Cluster, ClusterDir)> neighborClusters;
    public bool allJobDone;




    public NativeArray<Grid> grids;

    public int2 topRight;
    public int2 bottomLeft;
    public int2 clusterPos;
    public struct PathTag : IEquatable<PathTag>
    {
        public int2 fromPos;
        public int2 endPos;

        public bool Equals(PathTag other)
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

    /// <summary>
    /// 左下角，和右上角
    /// </summary>
    /// <param name="bottomLeft">左下角</param>
    /// <param name="topRight">右上角</param>
    public Cluster(int2 bottomLeft, int2 topRight)
    {
        this.bottomLeft = bottomLeft;
        this.topRight = topRight;
        this.clusterPos = StaticData.TransferIndex(bottomLeft);


        portalNodes = new List<PortalNode>();
        // pathJobs = new Dictionary<BuildPathJob, JobHandle>();
        // pathJobs = new Dictionary<Edge, JobHandle>();
        grids = new NativeArray<Grid>(StaticData.ClusterWidth * StaticData.ClusterWidth, Allocator.Persistent);
        for (int i = 0; i < StaticData.ClusterWidth; i++)
        {
            for (int j = 0; j < StaticData.ClusterWidth; j++)
            {
                // var newGrid = new Grid();
                // newGrid.Init();
                grids[i * StaticData.ClusterWidth + j] = new Grid(bottomLeft, new int2(i, j));
            }
        }
    }
    public void Init()
    {
        GeneratePortalNode();
        GenerateEdges();
    }
    public void AddClusterNeighbor(List<Cluster> neighbors)
    {
        // neighborClusters = neighbors;
        neighborClusters = neighbors.Select(i => (i, GetDir(i.clusterPos)));


        ClusterDir GetDir(int2 neighborPos)
        {
            var dir = neighborPos - clusterPos;
            var index = StaticData.Index4;
            if (dir.MyEquals(index[0]))
            {
                return ClusterDir.Top;
            }
            else if (dir.MyEquals(index[1]))
            {
                return ClusterDir.Right;
            }
            else if (dir.MyEquals(index[2]))
            {
                return ClusterDir.Left;
            }
            else
            {
                return ClusterDir.Bottom;
            }


        }

    }


    public void OnClusterChange()
    {
        allJobDone = false;
        DeleteEdge();
        DeletePortalNode();

        GeneratePortalNode();
        GenerateEdges(true);
    }

    public void DeletePortalNode()
    {
        portalNodes.Clear();
    }
    private void DeleteEdge()
    {
        foreach (var node in portalNodes)
        {
            foreach (var edge in node.edges)
            {
                if (edge.weight == 10)
                {
                    edge.endNode.edges.Remove(edge);
                }
            }

        }
    }
    public void GeneratePortalNode()
    {
        var topLeft = new int2(bottomLeft.x, topRight.y);
        var bottomRight = new int2(topRight.x, bottomLeft.y);

        portalNodes.AddRange(CheckPortalNode(topLeft - bottomLeft, new int2(1, 0)));
        portalNodes.AddRange(CheckPortalNode(topRight - bottomLeft, new int2(0, -1)));
        portalNodes.AddRange(CheckPortalNode(bottomRight - bottomLeft, new int2(-1, 0)));
        portalNodes.AddRange(CheckPortalNode(bottomLeft - bottomLeft, new int2(0, 1)));

        List<PortalNode> CheckPortalNode(int2 from, int2 dir)
        {
            List<PortalNode> tempPortalNodes = new List<PortalNode>();
            int2 currentFrom = from;
            int2 currentEnd = from;
            for (int2 i = int2.zero; i.ScalarAbs(dir) < StaticData.ClusterWidth; i += dir)
            {
                var currentIndex = from + i;

                if (grids.GetItem(currentIndex, ClusterWidth).isWalkable)
                {
                    currentEnd = currentIndex;
                }
                else
                {
                    if (!currentFrom.MyEquals(currentIndex))
                    {
                        var portalNode = new PortalNode(grids.GetSlice(currentFrom, currentEnd, dir), dir);



                        tempPortalNodes.Add(portalNode);
                    }
                    currentFrom = currentIndex + dir;
                }

            }
            if (currentFrom.ScalarAbs(dir) < StaticData.ClusterWidth)
                tempPortalNodes.Add(new PortalNode(grids.GetSlice(currentFrom, currentEnd, dir), dir));




            return tempPortalNodes;

        }





    }

    public void GenerateEdges(bool needAdd = false)
    {
        int ses = 1;
        //generate connect edge
        foreach (var (i, dir) in neighborClusters)
        {
            for (int j = 0; j < i.portalNodes.Count; j++)
            {
                for (int k = 0; k < portalNodes.Count; k++)
                {
                    var kk = portalNodes[k];
                    var jj = i.portalNodes[j];
                    if ((int)dir == (int)kk.dir
                    && (int)kk.dir + (int)jj.dir == 3
                    && kk.Near(jj, out Grid outputGrid, out Grid inputGrid))
                    {

                        // portalNodes.Add(jj);
                        portalNodes[k].edges.Add(new Edge(kk, jj, 10, outputGrid, inputGrid));

                        if (needAdd)
                            i.portalNodes[j].edges.Add(new Edge(jj, kk, 10, inputGrid, outputGrid));


                    }
                }
            }


        }
        var ttt = portalNodes;
        Debug.Log(ttt.Count);



        //generate Internal edge
        calculatePath.Clear();

        for (int i = 0; i < portalNodes.Count; i++)
            for (int j = i + 1; j < portalNodes.Count; j++)
            {

                var from = portalNodes[i];
                var end = portalNodes[j];
                if (from.Collinear(end)) continue;
                var doneEvent = new ManualResetEvent(false);
                calculatePath.Add(doneEvent);
                var edgeFrom2End = new Edge(from, end, doneEvent);


                edgeFrom2End.BuildPath(new object[] { from.midGrid, end.midGrid, grids });


                from.edges.Add(edgeFrom2End);


                var edgeEnd2From = new Edge(end, from, doneEvent);
                edgeEnd2From.path =  edgeFrom2End.path.ReverseNativeList();
                end.edges.Add(edgeEnd2From);

                // ThreadPool.QueueUserWorkItem(edge.BuildPath, new object[] { from.midGrid, end.midGrid, grids });
                // edge.doneEvent.WaitOne();

            }



        // WaitHandle.WaitAll(calculatePath.ToArray(), 10000);
        int b = 1;


    }





    public PortalNode ConnectGridToBorderNode(Grid grid)
    {


        PortalNode tempPortalNode = new PortalNode(grid);
        List<ManualResetEvent> doneEventList = new List<ManualResetEvent>();

        foreach (var node in portalNodes)
        {
            var doneEvent = new ManualResetEvent(false);
            doneEventList.Add(doneEvent);
            var EdgeToTempPortalNode = new Edge(node, tempPortalNode, doneEvent);
            node.edges.Add(EdgeToTempPortalNode);
            EdgeToTempPortalNode.BuildPath(new object[] { node.midGrid, grid, grids });


            var EdgeToBorderNode = new Edge(tempPortalNode, node, doneEvent);
            EdgeToBorderNode.path = EdgeToTempPortalNode.path.ReverseNativeList();
            tempPortalNode.edges.Add(EdgeToBorderNode);

            // ThreadPool.QueueUserWorkItem(tempEdge.BuildPath, new object[] { node.midGrid, grid, grids });
            // doneEvent.WaitOne();


        }
        // WaitHandle.WaitAll(doneEventList.ToArray());
        return tempPortalNode;

    }


}
