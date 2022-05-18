using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System.Threading;

public enum PortalNodeDir
{
    Top = 2,
    Bottom = 1,
    Left = 0,
    Right = 3
}

public class PortalNode : IComparable<PortalNode>, IEquatable<PortalNode>
{
    public int2 pos { get => midGrid.worldPos; }
    public Grid midGrid;
    public List<Grid> grids;

    public PortalNodeDir dir;
    public int gcost, hcost, fcost;


    public bool Collinear(PortalNode other)
    {
        return dir == other.dir;
    }

    public bool Near(PortalNode other, out Grid outputGrid, out Grid inputGrid)
    {
        outputGrid = default(Grid);
        inputGrid = default(Grid);
        for (int i = 0; i < grids.Count; i++)
        {
            for (int j = 0; j < other.grids.Count; j++)
            {
                if (grids[i].Near(other.grids[j]))
                {
                    outputGrid = grids[i];
                    inputGrid = other.grids[j];
                    return true;
                }
            }
        }
        return false;

    }

    public int CompareTo(PortalNode other)
    {
        if (fcost == 0) return 1;
        if (other.fcost == 0) return -1;

        return fcost - other.fcost;

    }

    public bool Equals(PortalNode other)
    {
        return other == this;
    }
    public override int GetHashCode()
    {
        return pos.x * StaticData.ClusterCount + pos.y;
    }

    public List<Edge> edges = new List<Edge>();



    public PortalNode(List<Grid> grids, int2 d)
    {
        this.grids = grids;
        this.dir = StaticData.GetDir(d);
        midGrid = grids[grids.Count / 2];
    }


    /// <summary>
    /// only for temp PortalNode
    /// </summary>
    /// <param name="grids"></param>
    /// <param name="d"></param>
    public PortalNode(Grid grid)
    {
        midGrid = grid;

    }




}
