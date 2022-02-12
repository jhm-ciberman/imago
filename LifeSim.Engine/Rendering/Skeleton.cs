using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Rendering;

public class Skeleton : IDisposable
{
    public const int MAX_NUMBER_OF_BONES = 64;
    public IList<Node3D> Joints { get; } = new List<Node3D>();
    public IList<Matrix4x4> InverseBindMatrices { get; }

    public Veldrid.ResourceSet ResourceSet { get; }

    public int BufferId => this._dataBlock.Buffer.Id;

    public uint BoneDataOffset => this._dataBlock.BlockIndex * MAX_NUMBER_OF_BONES;


    public Matrix4x4[] BonesMatrices { get; }

    public Matrix4x4 RootTransform { get; set; }

    private readonly SceneStorage _storage;
    private DataBlock _dataBlock;

    public Skeleton(IList<Node3D> joints, IList<Matrix4x4> inverseBindMatrices)
    {
        this._storage = Renderer.Instance.Storage;
        this.Joints = joints;
        this.InverseBindMatrices = inverseBindMatrices;
        this.BonesMatrices = new Matrix4x4[this.Joints.Count];
        this._dataBlock = this._storage.RequestSkeletonDataBlock();
        this.ResourceSet = this._dataBlock.Buffer.ResourceSet;
        this._storage.RegisterSkeleton(this);
    }


    public void Update()
    {
        Matrix4x4.Invert(this.RootTransform, out Matrix4x4 inverseMeshWorldMatrix);

        for (int i = 0; i < this.Joints.Count; i++)
        {
            this.BonesMatrices[i] = this.InverseBindMatrices[i] * this.Joints[i].WorldMatrix * inverseMeshWorldMatrix;
        }

        this._dataBlock.WriteSpan<Matrix4x4>(this.BonesMatrices);
    }

    public void Dispose()
    {
        this._dataBlock.FreeBlock();
        this._storage.UnregisterSkeleton(this);
    }
}