using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GPUInstance
{
    protected Mesh mesh;
    protected Material material;
    protected int layer;
    protected float time;

    public GPUInstance(Mesh mesh, Material material, int layer, float time)
    {
        this.mesh = mesh;
        this.material = material;
        this.layer = layer;
        this.time = time;
    }

    public virtual void UpdateInstance(float deltaTime)
    {
        time -= deltaTime;
        
        if (time <= 0)
        {
            GPUInstanceManager.RemoveInstance(this);
        }
    }
}

public class GPUInstanceStatic : GPUInstance
{
    private readonly Vector3 position;
    private readonly Quaternion rotation;

    public GPUInstanceStatic(Vector3 position, Quaternion rotation,
        Mesh mesh, Material material, int layer, float time) : base(mesh, material, layer, time)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public override void UpdateInstance(float deltaTime)
    {
        base.UpdateInstance(deltaTime);

        Graphics.DrawMesh(mesh, position, rotation, material, layer);
    }
}
