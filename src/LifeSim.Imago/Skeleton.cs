using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.Rendering.Buffers;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago;

public class Skeleton : IDisposable
{
    public const int MAX_NUMBER_OF_BONES = 64;
    public IList<Node3D> Joints { get; } = new List<Node3D>();

    public bool IsDisposed { get; private set; } = false;

    public IList<Matrix4x4> InverseBindMatrices { get; }

    public Veldrid.ResourceSet ResourceSet { get; }

    public uint BoneDataOffset => this._dataBlock.BlockIndex * MAX_NUMBER_OF_BONES;


    public Matrix4x4[] BonesMatrices { get; }

    public Matrix4x4 InverseRootTransform { get; set; }

    private DataBlock _dataBlock;

    public Skeleton(IList<Node3D> joints, IList<Matrix4x4> inverseBindMatrices)
    {
        this.Joints = joints;
        this.InverseBindMatrices = inverseBindMatrices;
        this.BonesMatrices = new Matrix4x4[this.Joints.Count];
        this._dataBlock = Renderer.Instance.RequestSkeletonDataBlock();
        this.ResourceSet = this._dataBlock.Buffer.ResourceSet;
        Renderer.Instance.RegisterDisposable(this);
    }


    public void Update()
    {
        for (int i = 0; i < this.Joints.Count; i++)
        {
            this.BonesMatrices[i] = this.InverseBindMatrices[i] * this.Joints[i].WorldMatrix * this.InverseRootTransform;
        }

        this._dataBlock.WriteSpan<Matrix4x4>(this.BonesMatrices);
    }

    public void Dispose()
    {
        if (this.IsDisposed) return;
        this.IsDisposed = true;

        this._dataBlock.Dispose();
        Renderer.Instance.UnregisterDisposable(this);
    }
}
