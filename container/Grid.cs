using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public struct Grid : IComparable<Grid>, IEquatable<Grid>
{
    public int2 worldPos;

    public bool isWalkable;
    public int2 localPos;
    public int gcost;
    public int hcost;
    public int fcost;
    public int2 camefrom;
    public int CompareTo(Grid other)
    {
        if (fcost == 0) return 1;
        if (other.fcost == 0) return -1;

        return fcost - other.fcost;
    }
    public Grid(int2 bottomLeft, int2 pos, int v = int.MaxValue, bool isWalkable = true)
    {
        this.worldPos = pos + bottomLeft;
        this.localPos = pos;
        this.isWalkable = isWalkable;
        camefrom = -1;
        gcost = v;
        hcost = 0;
        fcost = v;


    }

    public void Init(int2 pos)
    {

    }
    public bool Collinear(Grid other)
    {
        return worldPos.x == other.worldPos.x ||
                worldPos.y == other.worldPos.y;
    }
    public bool Near(Grid other)
    {
        var x1 = worldPos.x;
        var x2 = other.worldPos.x;
        var y1 = worldPos.y;
        var y2 = other.worldPos.y;



        return (x1 == x2 && math.abs(y1 - y2) == 1)
            || (y1 == y2 && math.abs(x1 - x2) == 1);

    }

    // public bool Equals(Grid other)
    // {
    //     throw new NotImplementedException();
    // }

    public bool Equals(Grid other)
    {
        return worldPos.MyEquals(other.worldPos);
    }



    public override int GetHashCode()
    {
        return worldPos.x * StaticData.ClusterWidth + worldPos.y;
    }
    public override string ToString()
    {
        return fcost.ToString();
    }


}
