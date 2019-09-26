using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public struct CuboidData
{
    public Vector4 min;
    public Vector4 max;
}

public class CuboidCantainer
{
    public Vector3 Center { get { return this.bounds.center; } }
    public Vector3 Size { get { return this.bounds.size; } }
    public Bounds Bound { get { return this.bounds; } }
    //CPU Bounds
    protected Bounds bounds;
    public CuboidCantainer() : this(Vector3.zero, Vector3.zero)
    {
    }
    public CuboidCantainer(Vector3 center, Vector3 size)
    {
        this.bounds = new Bounds(center, size);
    }

    public bool Contains(CuboidCantainer other)
    {
        return bounds.Contains(other.bounds.min) && bounds.Contains(other.bounds.max);
    }

    public virtual void OnDrawGizmos(Color color)
    {
        var old = Gizmos.color;
        Gizmos.color = color;
        Gizmos.DrawWireCube(this.bounds.center, this.bounds.size);
        Gizmos.color = old;
    }
}
public class Cuboid<T> : CuboidCantainer
{
    public Cuboid(Vector3 center, Vector3 size) : base(center, size)
    {
    }
    static public int GetGPUSizeInByte()
    {
        return Marshal.SizeOf<T>();
    }

}

public class ParticleCuboid : Cuboid<CuboidData>
{
    public ParticleCuboid(Vector3 center, Vector3 size) : base(center, size)
    {
    }
}
